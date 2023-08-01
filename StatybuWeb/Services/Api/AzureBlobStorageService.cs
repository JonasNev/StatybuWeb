using Azure.Storage.Blobs;
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
            _connectionString = _azureKeyVaultService.GetSecretFromKeyVault(Constants.AzureConstants.KeyVaultSecretNames.ConnectionString).GetAwaiter().GetResult();
            _blobContainer = _azureKeyVaultService.GetSecretFromKeyVault(Constants.AzureConstants.KeyVaultSecretNames.BlobContainer).GetAwaiter().GetResult();
        }

        public async Task<List<Picture>> GetImagesFilesFromBlobStorage()
        {
            try
            {
                BlobContainerClient containerClient = await GetAzureBlobContainerClientFromSecrets();

                List<Picture> pictures = new List<Picture>();

                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    if (blobItem != null)
                    {
                        BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                        if (blobClient != null)
                        {
                            pictures.Add(new Picture()
                            {
                                Uri = blobClient.Uri,
                                Name = blobItem.Name,
                                Extension = Path.GetExtension(blobItem.Name)
                            });
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to get BlobClient for blobItem: {blobItem.Name}");
                        }
                    }
                }

                _logger.LogInformation($"Retrieved {pictures.Count} pictures from blob storage");
                return pictures;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting image files from Azure Blob storage");
                throw new InvalidOperationException("An error occurred while getting image files from Azure Blob storage", ex);
            }
        }

        public async Task UploadFileToBlobStorage(IFormFile file)
        {
            try
            {
                if (file != null)
                {

                    BlobContainerClient containerClient = await GetAzureBlobContainerClientFromSecrets();

                    using (var stream = file.OpenReadStream())
                    {
                        string uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                        // Compress the image before uploading
                        using (var compressedStream = await CompressImageAsync(stream))
                        {
                            BlobClient blobClient = containerClient.GetBlobClient(uniqueName);

                            await blobClient.UploadAsync(compressedStream, overwrite: true);
                        }
                    }

                    _logger.LogInformation("File uploaded to Azure Blob storage successfully.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file to Azure Blob storage: {ex.Message}");
                throw new InvalidOperationException("Error uploading file to Azure Blob storage", ex);
            }
        }

        private async Task<Stream> CompressImageAsync(Stream imageStream)
        {
            using (var image = Image.Load(imageStream))
            {
                // Apply compression settings to reduce the size of the image
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(800, 600), // Adjust the size as per your requirements
                    Mode = ResizeMode.Max // Choose the appropriate resize mode
                }));

                var compressedStream = new MemoryStream();

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
                BlobContainerClient containerClient = await GetAzureBlobContainerClientFromSecrets();

                BlobClient blobClient = containerClient.GetBlobClient(guid);

                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogInformation($"Blob with GUID '{guid}' does not exist in Azure Blob storage.");
                    return null;
                }

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
                throw new InvalidOperationException("Error retrieving file URL from Azure Blob storage", ex);
            }
        }

        public async Task<BlobContainerClient> GetAzureBlobContainerClientFromSecrets()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

            var blobContainer = await Task.FromResult(blobServiceClient.GetBlobContainerClient(_blobContainer));

            if (blobContainer == null)
            {
                _logger.LogError("BlobContainerClient is null");
                throw new ArgumentNullException($"Blob container {nameof(_blobContainer)} is null, check connection string or blobcontainer name in azure secrets.");
            }

            return blobContainer;
        }
    }
}
