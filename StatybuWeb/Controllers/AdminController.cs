using Microsoft.AspNetCore.Authorization;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using StatybuWeb.Services.Api;
using System.Security.Claims;
using StatybuWeb.Dto;
using StatybuWeb.Services.Auth0;
using StatybuWeb.Constants;

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
            if (!IsUserAuthorized(Auth0Constants.Roles.Admin))
            {
                return View("Unauthorized");
            }
            
            if (file != null && file.Length > 0)
            {
                await _azureBlobStorageService.UploadFileToBlobStorage(file);
            }

            return RedirectToAction("Index");

        }

        [Authorize(AuthenticationSchemes = "Auth0")]
        public async Task<ActionResult> ImageActions()
        {
            if (IsUserAuthorized(Auth0Constants.Roles.Admin))
            {
                return View(await _azureBlobStorageService.GetImagesFilesFromBlobStorage());
            }

            return View("Unauthorized");
        }

        [Authorize(AuthenticationSchemes = "Auth0")]
        [HttpPost]
        public async Task<ActionResult> DeleteImages(List<Picture> fileNames)
        {
            if (!IsUserAuthorized(Auth0Constants.Roles.Admin))
            {
                return View("Unauthorized");
            }

            BlobContainerClient containerClient = await _azureBlobStorageService.GetAzureBlobContainerClientFromSecrets();
            foreach (var picture in fileNames)
            {
                if (picture.Selected)
                {
                    try
                    {
                        BlobClient blobClient = containerClient.GetBlobClient(picture.Name);

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
    }
}
