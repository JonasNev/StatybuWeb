using Microsoft.AspNetCore.Mvc;
using StatybuWeb.Services.Steam;

namespace StatybuWeb.Controllers
{
    public class SteamController : Controller
    {
        private readonly ISteamService _steamService;

        public SteamController(ISteamService steamService)
        {
            _steamService = steamService;
        }

        [HttpGet]
        [Route("Steam/Friends/{steamId?}")]
        public async Task<IActionResult> Friends(string steamId)
        {
            var model = await _steamService.GetFriendsList(steamId);
            return PartialView(model);
        }
    }
}
