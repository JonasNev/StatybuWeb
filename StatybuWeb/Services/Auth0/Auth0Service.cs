using Azure.Storage.Blobs;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StatybuWeb.Constants;
using StatybuWeb.Dto;
using StatybuWeb.Models.Auth0;
using StatybuWeb.Services.Api;
using System.Net.Http.Headers;
using System.Text;

namespace StatybuWeb.Services.Auth0
{
    public class Auth0Service : IAuth0Service
    {
        private readonly HttpClient _httpClient;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private readonly string _auth0ManagementToken;
        private readonly ILogger<Auth0Service> _logger;

        public Auth0Service(IAzureKeyVaultService azureKeyVaultService, ILogger<Auth0Service> logger)
        {
            _azureKeyVaultService = azureKeyVaultService;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_azureKeyVaultService.GetSecretFromKeyVault(AzureConstants.KeyVaultSecretNames.BaseAuth0Address).GetAwaiter().GetResult());
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _auth0ManagementToken = InitializeAuth0ManagementToken().GetAwaiter().GetResult();
            _logger = logger;
        }

        private async Task<string> InitializeAuth0ManagementToken()
        {
            try
            {
                return await _azureKeyVaultService.GetSecretFromKeyVault(AzureConstants.KeyVaultSecretNames.Auth0ManagementToken100D);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while initializing Auth0 management token: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Role>> GetUserRoles(string userId)
        {
            var roles = new List<Role>();
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _auth0ManagementToken);

                // Call Auth0 Management API to get user roles
                var response = await _httpClient.GetAsync($"api/v2/users/{userId}/roles");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                roles = JsonConvert.DeserializeObject<List<Role>>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while retrieving user roles: {ex.Message}");
            }

            return roles;
        }

        public async Task<User> GetUser(string userId)
        {
            var user = new User();
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _auth0ManagementToken);

                // Call Auth0 Management API to get user roles
                var response = await _httpClient.GetAsync($"/api/v2/users/{userId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<User>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while retrieving user roles: {ex.Message}");
            }

            return user;
        }

        public async Task UpdateUser(string userId, User newData, User user)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _auth0ManagementToken);
                var updateDto = PopulateNewData(newData);

                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var json = JsonConvert.SerializeObject(updateDto, serializerSettings);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                // Call Auth0 Management API to update user data
                var response = await _httpClient.PatchAsync($"/api/v2/users/{userId}", httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error occurred while updating user data: {response.StatusCode} - {errorMessage}");
                    // Additional error handling or logging can be performed here
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while updating user data: {ex.Message}");
                // Additional error handling or logging can be performed here
            }
        }

        private UserUpdateDto PopulateNewData(User newData)
        {
            var user = new UserUpdateDto()
            {
                User_metadata = new UserUpdateDto.User_Metadata()
                {
                    Username = newData?.User_metadata?.Username,
                    Picture = newData?.User_metadata?.Picture,
                    Nickname = newData?.User_metadata?.Nickname,
                    SteamId = newData?.User_metadata?.SteamId
                }
            };
            return user;
        }
    }
}
