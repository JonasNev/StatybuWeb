using Azure.Storage.Blobs;
using Newtonsoft.Json;
using StatybuWeb.Constants;
using StatybuWeb.Models.Auth0;
using StatybuWeb.Services.Api;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace StatybuWeb.Services.Auth0
{
    public class Auth0Service : IAuth0Service
    {
        private readonly HttpClient _httpClient;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly string _auth0ManagementToken;

        public Auth0Service(IHttpContextAccessor httpContextAccessor, IAzureBlobStorageService azureBlobStorageService)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://dev-4b6q5857i7jcmzkw.eu.auth0.com/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _azureBlobStorageService = azureBlobStorageService;
            _auth0ManagementToken = InitializeAuth0ManagementToken().GetAwaiter().GetResult();
        }

        private async Task<string> InitializeAuth0ManagementToken()
        {
            try
            {
                return await _azureBlobStorageService.GetSecretFromKeyVault(AzureConstants.KeyVaultSecretNames.Auth0ManagementToken10D);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while initializing Auth0 management token: {ex.Message}");
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
                Console.WriteLine($"Error occurred while retrieving user roles: {ex.Message}");
            }

            return roles;
        }
    }
}
