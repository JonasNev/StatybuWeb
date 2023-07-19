using Newtonsoft.Json;
using StatybuWeb.Constants;
using StatybuWeb.Models.Steam;
using StatybuWeb.Services.Api;

namespace StatybuWeb.Services.Steam
{
    public class SteamService : ISteamService
    {
        private readonly HttpClient _httpClient;
        private readonly IAzureKeyVaultService _azureKeyVaultService;

        public SteamService(HttpClient httpClient, 
            IAzureKeyVaultService azureKeyVaultService)
        {
            _httpClient = httpClient;
            _azureKeyVaultService = azureKeyVaultService;
        }

        public async Task<SteamFriends> GetFriendsListAsync(string steamId)
        {
            var apiKey = _azureKeyVaultService.GetSecretFromKeyVault(SteamConstants.KeyVaultSecretNames.SteamApiKey)?.Result;
            var response = await _httpClient.GetStringAsync($"{SteamConstants.FriendListApiBaseUrl}?key={apiKey}&steamid={steamId}");
            var result = JsonConvert.DeserializeObject<SteamFriends>(response);
            return result;
        }
    }
}
