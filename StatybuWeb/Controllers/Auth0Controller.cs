using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using StatybuWeb.Services.Auth0;
using StatybuWeb.Models.Auth0;
using StatybuWeb.Services.Api;

namespace StatybuWeb.Controllers
{
    public class Auth0Controller : Controller
    {
        private readonly IAuth0Service _auth0Service;
        private readonly ILogger<Auth0Controller> _logger;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        public Auth0Controller(IAuth0Service auth0Service,
            ILogger<Auth0Controller> logger,
            IAzureKeyVaultService azureKeyVaultService)
        {
            _auth0Service = auth0Service;
            _logger = logger;
            _azureKeyVaultService = azureKeyVaultService;
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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response?.Cookies?.Delete(".AspNetCore.Cookies");

            var auth0Domain = _azureKeyVaultService.GetSecretFromKeyVault(Constants.AzureConstants.KeyVaultSecretNames.Auth0Domain)?.Result;
            var auth0ClientId = _azureKeyVaultService.GetSecretFromKeyVault(Constants.AzureConstants.KeyVaultSecretNames.Auth0ClientId)?.Result;

            if (string.IsNullOrEmpty(auth0Domain) || string.IsNullOrEmpty(auth0ClientId))
            {
                _logger.LogError("Error: Required Auth0 values from Azure Vault are null or empty");
                return View("Error");
            }

            var redirectUri = Url.Action("Index", "Home", null, "https") ?? Url.Action("Error", "Home", null, "https");

            if (redirectUri == null)
            {
                _logger.LogError("Error: Could not generate a valid redirect URI");
                return View("Error");
            }

            return Redirect($"https://{auth0Domain}/v2/logout?client_id={auth0ClientId}&returnTo={Uri.EscapeDataString(redirectUri)}");
        }


        [Authorize(AuthenticationSchemes = "Auth0")]
        [Route("/account/profile")]
        public async Task<IActionResult> Profile()
        {
            string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return View("Error");
            }

            var user = await _auth0Service.GetUser(userId);

            if (user == null)
            {
                return View("Error");
            }

            return View(user);
        }

        [Authorize(AuthenticationSchemes = "Auth0")]
        public IActionResult UpdateProfile(User user)
        {
            if (!ModelState.IsValid)
            {
                // The model is not valid, return the same view to display the validation errors
                return View("Profile", user);
            }

            string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return View("Error");
            }
            var currentUserData = _auth0Service.GetUser(userId).Result;
            _auth0Service.UpdateUser(userId, user, currentUserData);
            return RedirectToAction("Profile", user);
        }
    }
}
