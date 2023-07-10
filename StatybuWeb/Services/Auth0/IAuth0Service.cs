using StatybuWeb.Models.Auth0;

namespace StatybuWeb.Services.Auth0
{
    public interface IAuth0Service
    {
        Task<List<Role>> GetUserRoles(string userId);
        Task<User> GetUser(string userId);
        Task UpdateUser(string userId, User newData, User user);
    }
}
