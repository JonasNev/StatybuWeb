using StatybuWeb.Models.Auth0;

namespace StatybuWeb.Services.Auth0
{
    public interface IAuth0Service
    {
        Task<List<Role>> GetUserRoles(string userId);
    }
}
