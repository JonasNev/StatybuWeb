using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
    }
}
