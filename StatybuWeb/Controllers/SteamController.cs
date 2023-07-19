using Microsoft.AspNetCore.Mvc;
using StatybuWeb.Services.Steam;

namespace StatybuWeb.Controllers
{
    public class SteamController : Controller
    {
        private readonly SteamService _steamService;

        public SteamController(SteamService steamService)
        {
            _steamService = steamService;
        }

        public async Task<IActionResult> Friends(string id)
        {
            var model = await _steamService.GetFriendsListAsync(id);
            return View(model);
        }
    }
}
