using StatybuWeb.Constants;
using StatybuWeb.Models.Steam;
using StatybuWeb.Services.Api;
using StatybuWeb.Services.Steam;

public class SteamService : ISteamService
{
    private readonly HttpClient _httpClient;
    private readonly IAzureKeyVaultService _azureKeyVaultService;
    private readonly ILogger<SteamService> _logger;
    private string _apiKey;

    public SteamService(IAzureKeyVaultService azureKeyVaultService, HttpClient httpClient, ILogger<SteamService> logger)
    {
        _httpClient = httpClient;
        _azureKeyVaultService = azureKeyVaultService;
        _logger = logger;
        _apiKey = _azureKeyVaultService.GetSecretFromKeyVault(SteamConstants.KeyVaultSecretNames.SteamApiKey).GetAwaiter().GetResult();
    }

    public async Task<IEnumerable<Player>> GetFriendsList(string steamId)
    {
        try
        {
            var steamFriends = await _httpClient.GetAsync($"{SteamConstants.FriendListApiBaseUrl}?key={_apiKey}&steamid={steamId}&relationship=friend");

            if (!steamFriends.IsSuccessStatusCode)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting the friends list");
            throw;
        }
    }

    public async Task<IEnumerable<Player>> GetFriendSummaries(IEnumerable<string> steamIds)
    {
        try
        {
            var playersummariesHttpMessage = await _httpClient.GetAsync($"{SteamConstants.PlayerSummariesBaseUrl}?key={_apiKey}&steamids={string.Join(",", steamIds)}");
            playersummariesHttpMessage.EnsureSuccessStatusCode();
            var steamFriendsStatusJson = await playersummariesHttpMessage.Content.ReadAsStringAsync();
            var steamFriendInfo = System.Text.Json.JsonSerializer.Deserialize<SteamFriendInfo>(steamFriendsStatusJson);
            if (steamFriendInfo?.response?.players == null)
            {
                _logger.LogError("Deserialized SteamFriendInfo object or its components are null");
                throw new InvalidOperationException("Deserialized SteamFriendInfo object or its components are null");
            }
            return steamFriendInfo.response.players;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting the friend summaries");
            throw;
        }
    }
}
