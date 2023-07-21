using StatybuWeb.Constants;
using StatybuWeb.Models.Steam;
using StatybuWeb.Services.Api;
using System.Net.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Newtonsoft.Json;

namespace StatybuWeb.Services.Steam
{
    public class SteamService : ISteamService
    {
        private readonly HttpClient _httpClient;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private string _apiKey; 

        public SteamService(IAzureKeyVaultService azureKeyVaultService)
        {
            _httpClient = new HttpClient();
            _azureKeyVaultService = azureKeyVaultService;
            _apiKey = _azureKeyVaultService.GetSecretFromKeyVault(SteamConstants.KeyVaultSecretNames.SteamApiKey).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<Player>> GetFriendsList(string steamId)
        {
            var steamFriends = await _httpClient.GetAsync($"{SteamConstants.FriendListApiBaseUrl}?key={_apiKey}&steamid={steamId}&relationship=friend");
            var steamResponseJson = await steamFriends.Content.ReadAsStringAsync();
            var steamResponse = System.Text.Json.JsonSerializer.Deserialize<SteamResponse>(steamResponseJson);
            var friends = steamResponse.friendslist.friends;

            var batchSize = 100;
            var taskList = new List<Task<IEnumerable<Player>>>();
            for (int i = 0; i < friends.Length; i += batchSize)
            {
                var batch = friends.Skip(i).Take(batchSize).Select(friend => friend.steamid);
                taskList.Add(GetFriendSummaries(batch));
            }

            var playerBatches = await Task.WhenAll(taskList);
            var players = playerBatches.SelectMany(players => players);
            return players.Where(x => x.personastate != PersonaState.Offline).OrderBy(player => player.personastate);
        }

        public async Task<IEnumerable<Player>> GetFriendSummaries(IEnumerable<string> steamIds)
        {
            var playersummariesHttpMessage = await _httpClient.GetAsync($"{SteamConstants.PlayerSummariesBaseUrl}?key={_apiKey}&steamids={string.Join(",", steamIds)}");
            playersummariesHttpMessage.EnsureSuccessStatusCode();
            var steamFriendsStatusJson = await playersummariesHttpMessage.Content.ReadAsStringAsync();
            var steamFriendInfo = System.Text.Json.JsonSerializer.Deserialize<SteamFriendInfo>(steamFriendsStatusJson);
            return steamFriendInfo.response.players;
        }
    }
}
