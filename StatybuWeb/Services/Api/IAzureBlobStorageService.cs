using Azure.Storage.Blobs;
using StatybuWeb.Dto;

namespace StatybuWeb.Services.Api
{
    public interface IAzureBlobStorageService
    {
        Task UploadFileToBlobStorage(IFormFile fileToUpload);
        Task<Picture?> GetFileUrlFromBlobStorageAsync(string guid);
        Task<List<Picture>> GetImagesFilesFromBlobStorage();
        Task<BlobContainerClient> GetAzureBlobContainerClientFromSecrets();
        Task<string> GetSecretFromKeyVault(string secretName);
    }
}
