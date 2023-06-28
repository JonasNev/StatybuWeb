using Microsoft.AspNetCore.Mvc;
using StatybuWeb.Services.Api;

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
        public async Task<ActionResult> Index()
        {
            return View(await _azureBlobStorageService.GetImagesFilesFromBlobStorage());
        }

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
