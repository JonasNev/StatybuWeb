using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using StatybuWeb.Services.Auth0;
using StatybuWeb.Models.Auth0;

namespace StatybuWeb.Controllers
{
    public class Auth0Controller : Controller
    {
        private readonly IAuth0Service _auth0Service;
        public Auth0Controller(IAuth0Service auth0Service)
        {
            _auth0Service = auth0Service;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("/account/signup")]
        public async Task Signup(string returnUrl = "/home")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithParameter("screen_hint", "signup")
                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        [Route("/account/login")]
        public async Task Login(string returnUrl = "/home")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                // Indicate here where Auth0 should redirect the user after a login.
                // Note that the resulting absolute Uri must be added to the
                // **Allowed Callback URLs** settings for the app.
                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        [Authorize(AuthenticationSchemes = "Auth0")]
        [Route("/account/logout")]
        public async Task Logout()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be added to the
                // **Allowed Logout URLs** settings for the app.
                .WithRedirectUri(Url.Action("Index", "Home", null, "https"))
                .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete(".AspNetCore.Cookies");
        }

        [Authorize(AuthenticationSchemes = "Auth0")]
        [Route("/account/profile")]
        public IActionResult Profile()
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = _auth0Service.GetUser(userId).Result;
            return View(user);
        }

        [Authorize(AuthenticationSchemes = "Auth0")]
        public IActionResult UpdateProfile(User user)
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var currentUserData = _auth0Service.GetUser(userId).Result;
            _auth0Service.UpdateUser(userId, user, currentUserData);
            return RedirectToAction("Profile", user);
        }
    }
}
