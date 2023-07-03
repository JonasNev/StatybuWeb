using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using SixLabors.ImageSharp.Formats.Jpeg;
using StatybuWeb.Dto;

namespace StatybuWeb.Services.Api
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private static readonly Dictionary<string, string> SecretCache = new Dictionary<string, string>();
        private static readonly object CacheLock = new object();
        public async Task<List<Picture>> GetImagesFilesFromBlobStorage()
        {
            BlobContainerClient containerClient = await GetAzureBlobContainerClientFromSecrets();

            List<Picture> imageUris = new List<Picture>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                imageUris.Add(new Picture()
                {
                    Uri = blobClient.Uri,
                    Name= blobItem.Name,
                    Extension = Path.GetExtension(blobItem.Name)
                });
            }

            return imageUris;
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
                        using (var compressedStream = CompressImage(stream))
                        {
                            // Get a reference to a blob within the container
                            BlobClient blobClient = containerClient.GetBlobClient(uniqueName);

                            // Upload the file to Azure Blob storage
                            await blobClient.UploadAsync(compressedStream, overwrite: true);
                        }
                    }

                    Console.WriteLine("File uploaded to Azure Blob storage successfully.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file to Azure Blob storage: {ex.Message}");
            }
        }

        private Stream CompressImage(Stream imageStream)
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
                image.Save(compressedStream, new JpegEncoder()); // Choose the appropriate encoder based on the desired output format (e.g., Jpeg, Png, etc.)

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
                    Console.WriteLine($"Blob with GUID '{guid}' does not exist in Azure Blob storage.");
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
                Console.WriteLine($"Error retrieving file URL from Azure Blob storage: {ex.Message}");
                return null;
            }
        }

        public async Task<BlobContainerClient> GetAzureBlobContainerClientFromSecrets()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(await GetSecretFromKeyVault(Constants.AzureConstants.KeyVaultSecretNames.ConnectionString));

            return await Task.FromResult(blobServiceClient.GetBlobContainerClient(await GetSecretFromKeyVault(Constants.AzureConstants.KeyVaultSecretNames.BlobContainer)));
        }


        private async Task<string> GetSecretFromKeyVault(string secretName)
        {
            // Check if the secret exists in the cache
            if (SecretCache.TryGetValue(secretName, out string cachedSecret))
            {
                return cachedSecret;
            }

            // Retrieve the secret from Key Vault outside the lock
            var credential = new DefaultAzureCredential();
            var client = new SecretClient(new Uri(Constants.AzureConstants.KeyVaultUrl), credential);

            KeyVaultSecret secret = await client.GetSecretAsync(secretName);

            lock (CacheLock)
            {
                // Double-check within the lock to prevent multiple threads from simultaneously updating the cache
                if (!SecretCache.ContainsKey(secretName))
                {
                    // Cache the retrieved secret
                    SecretCache[secretName] = secret.Value;
                }
            }

            return secret.Value;
        }
    }
}
