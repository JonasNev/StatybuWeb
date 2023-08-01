using Microsoft.AspNetCore.Mvc;
using StatybuWeb.Models;
using System.Diagnostics;

namespace StatybuWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            if (HttpContext.Items["Exception"] is Exception ex)

            {
                ViewData["ErrorMessage"] = ex.Message;
            }
            else
            {
                ViewData["ErrorMessage"] = "An error occurred while processing your request. Please try again later.";
            }

            return View();
        }
    }
}