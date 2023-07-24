using Microsoft.AspNetCore.Mvc;
using StatybuWeb.Services.Steam;
using System.Reflection;

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
            try
            {
                var model = await _steamService.GetFriendsList(steamId);
                return PartialView(model);
            }
            catch (Exception ex)
            {
                if (ex.Message == "User not found")
                    return NotFound("Error: The Steam ID does not exist.");
                else
                    return StatusCode(500, "Error: There was a server error.");
            }
        }
    }
}
