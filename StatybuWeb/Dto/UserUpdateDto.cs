using static StatybuWeb.Models.Auth0.User;

namespace StatybuWeb.Dto
{
    public class UserUpdateDto
    {
        public User_Metadata? User_metadata { get; set; }

        public class User_Metadata
        {
            public string? Username { get; set; }
            public string? Picture { get; set; }
            public string? Nickname { get; set; }
        }

    }
}
