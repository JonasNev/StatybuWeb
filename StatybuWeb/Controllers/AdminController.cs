using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using StatybuWeb.Services.Api;
using System.Security.Claims;

namespace StatybuWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAzureService _azureService;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        public AdminController(IAzureService azureService, IAzureBlobStorageService azureBlobStorageService)
        {
            _azureService = azureService;
            _azureBlobStorageService = azureBlobStorageService;
        }

        [Authorize(AuthenticationSchemes = "Auth0")]
        public async Task<ActionResult> Index()
        {
            return View(await _azureBlobStorageService.GetImagesFilesFromBlobStorage());
        }

        [Authorize(AuthenticationSchemes = "Auth0")]
        [HttpPost]
        public async Task<ActionResult> UploadImage(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                await _azureBlobStorageService.UploadFileToBlobStorage(file);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> DeleteImages(List<string> fileNames)
        {
            BlobContainerClient containerClient = await _azureBlobStorageService.GetAzureBlobContainerClientFromSecrets();
            foreach (string name in fileNames)
            {
                try
                {
                    // Get a reference to the file
                    BlobClient blobClient = containerClient.GetBlobClient(name);

                    // Delete the file
                    await blobClient.DeleteAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file': {ex.Message}");
                }
            }
            return RedirectToAction("Index");
        }
    }
}
