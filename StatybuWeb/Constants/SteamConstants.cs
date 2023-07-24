namespace StatybuWeb.Constants
{
    public static class SteamConstants
    {
        public const string FriendListApiBaseUrl = "https://api.steampowered.com/ISteamUser/GetFriendList/v0001/";
        public const string PlayerSummariesBaseUrl = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/";
        public static class KeyVaultSecretNames
        {
            public const string SteamApiKey = "SteamApiKey";
        }
    }
}
