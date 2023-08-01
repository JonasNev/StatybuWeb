using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using StatybuWeb.Services.Api;

namespace StatybuWeb.Controllers
{
    public class ApiController : Controller
    {
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        public ApiController(IAzureBlobStorageService azureBlobStorageService)
        {
            _azureBlobStorageService = azureBlobStorageService;
        }

        public async Task<ActionResult> Index()
        {
            var picture = await _azureBlobStorageService.GetFileUrlFromBlobStorageAsync("fa8d7ea8-69a4-4588-9eef-fa936a9cd41e");

            return View("~/Views/Api/Index.cshtml", picture);
        }
    }
}
