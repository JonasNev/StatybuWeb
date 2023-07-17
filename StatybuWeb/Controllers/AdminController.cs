using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using StatybuWeb.Services.Api;
using System.Security.Claims;
using StatybuWeb.Dto;
using StatybuWeb.Services.Auth0;

namespace StatybuWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IAuth0Service _auth0Service;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAzureBlobStorageService azureBlobStorageService, 
            IAuth0Service auth0Service,
            ILogger<AdminController> logger)
        {
            _azureBlobStorageService = azureBlobStorageService;
            _auth0Service = auth0Service;
            _logger = logger;
        }

        private bool IsUserAuthorized(string role)
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return false;
            }
            var userRoles = _auth0Service.GetUserRoles(userId).Result.Select(x => x.Name);
            return userRoles.Contains(role);
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
            if (IsUserAuthorized("Admin"))
            {
                if (file != null && file.Length > 0)
                {
                    await _azureBlobStorageService.UploadFileToBlobStorage(file);
                }
                return RedirectToAction("Index");
            }
            return View("Unauthorized");
        }

        [Authorize(AuthenticationSchemes = "Auth0")]
        public async Task<ActionResult> ImageActions()
        {
            if (IsUserAuthorized("Admin"))
            {
                return View(await _azureBlobStorageService.GetImagesFilesFromBlobStorage());
            }
            return View("Unauthorized");
        }

        [Authorize(AuthenticationSchemes = "Auth0")]
        [HttpPost]
        public async Task<ActionResult> DeleteImages(List<Picture> fileNames)
        {
            if (IsUserAuthorized("Admin"))
            {
                BlobContainerClient containerClient = await _azureBlobStorageService.GetAzureBlobContainerClientFromSecrets();
                foreach (var picture in fileNames)
                {
                    if (picture.Selected)
                    {
                        try
                        {
                            // Get a reference to the file
                            BlobClient blobClient = containerClient.GetBlobClient(picture.Name);

                            // Delete the file
                            await blobClient.DeleteAsync();

                            _logger.LogInformation($"Success deleting file': {picture.Name}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error deleting file': {ex.Message}");
                        }
                    }
                }
                return RedirectToAction("Index");
            }
            return View("Unauthorized");
        }
    }
}
