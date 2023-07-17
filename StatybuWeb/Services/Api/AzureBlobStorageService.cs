using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Formats.Jpeg;
using StatybuWeb.Dto;

namespace StatybuWeb.Services.Api
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private string _connectionString;
        private string _blobContainer;

        public AzureBlobStorageService(ILogger<AzureBlobStorageService> logger,
            IAzureKeyVaultService azureKeyVaultService)
        {
            _logger = logger;
            _azureKeyVaultService = azureKeyVaultService;
            // Fetch the secrets at the start of the application
            _connectionString = _azureKeyVaultService.GetSecretFromKeyVault(Constants.AzureConstants.KeyVaultSecretNames.ConnectionString).GetAwaiter().GetResult();
            _blobContainer = _azureKeyVaultService.GetSecretFromKeyVault(Constants.AzureConstants.KeyVaultSecretNames.BlobContainer).GetAwaiter().GetResult();
        }

        public async Task<List<Picture>> GetImagesFilesFromBlobStorage()
        {
            BlobContainerClient containerClient = new BlobContainerClient(_connectionString, _blobContainer);

            List<Picture> pictures = new List<Picture>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                pictures.Add(new Picture()
                {
                    Uri = blobClient.Uri,
                    Name = blobItem.Name,
                    Extension = Path.GetExtension(blobItem.Name)
                });
            }

            return pictures;
        }

        public async Task UploadFileToBlobStorage(IFormFile file)
        {
            try
            {
                if (file != null)
                {
                    BlobContainerClient containerClient = await GetAzureBlobContainerClientFromSecrets();

                    // Read the file as a stream
                    using (var stream = file.OpenReadStream())
                    {
                        // Create a unique name for the picture
                        string uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                        // Compress the image before uploading
                        using (var compressedStream = await CompressImageAsync(stream))
                        {
                            // Get a reference to a blob within the container
                            BlobClient blobClient = containerClient.GetBlobClient(uniqueName);

                            // Upload the file to Azure Blob storage
                            await blobClient.UploadAsync(compressedStream, overwrite: true);
                        }
                    }

                    _logger.LogInformation("File uploaded to Azure Blob storage successfully.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file to Azure Blob storage: {ex.Message}");
                throw; // Rethrow the exception for the calling code to handle
            }
        }

        private async Task<Stream> CompressImageAsync(Stream imageStream)
        {
            using (var image = SixLabors.ImageSharp.Image.Load(imageStream))
            {
                // Apply compression settings to reduce the size of the image
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(800, 600), // Adjust the size as per your requirements
                    Mode = ResizeMode.Max // Choose the appropriate resize mode
                }));

                // Create a new memory stream to store the compressed image
                var compressedStream = new MemoryStream();

                // Save the compressed image to the stream
                await image.SaveAsync(compressedStream, new JpegEncoder()); // Choose the appropriate encoder based on the desired output format (e.g., Jpeg, Png, etc.)

                // Rewind the stream to the beginning before returning it
                compressedStream.Position = 0;

                return compressedStream;
            }
        }

        public async Task<Picture?> GetFileUrlFromBlobStorageAsync(string guid)
        {
            try
            {
                // Get a reference to the container
                BlobContainerClient containerClient = await GetAzureBlobContainerClientFromSecrets();

                // Get a reference to the blob based on the given GUID
                BlobClient blobClient = containerClient.GetBlobClient(guid);

                // Check if the blob exists
                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogInformation($"Blob with GUID '{guid}' does not exist in Azure Blob storage.");
                    return null;
                }

                // Get the URL of the blob
                string blobUrl = blobClient.Uri.ToString();
                return new Picture()
                {
                    Uri = blobClient.Uri,
                    Name = blobClient.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving file URL from Azure Blob storage: {ex.Message}");
                throw; // Rethrow the exception for the calling code to handle
            }
        }

        public async Task<BlobContainerClient> GetAzureBlobContainerClientFromSecrets()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

            return await Task.FromResult(blobServiceClient.GetBlobContainerClient(_blobContainer));
        }
    }
}
