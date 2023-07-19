using StatybuWeb.Models.Steam;

namespace StatybuWeb.Services.Steam
{
    public interface ISteamService
    {
        Task<SteamFriends> GetFriendsListAsync(string steamId);
    }
}
