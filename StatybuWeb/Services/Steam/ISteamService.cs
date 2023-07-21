using StatybuWeb.Models.Steam;

namespace StatybuWeb.Services.Steam
{
    public interface ISteamService
    {
        Task<IEnumerable<Player>> GetFriendsList(string steamId);
        Task<IEnumerable<Player>> GetFriendSummaries(IEnumerable<string> steamIds);
    }
}