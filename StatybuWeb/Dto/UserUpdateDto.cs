using static StatybuWeb.Models.Auth0.User;

namespace StatybuWeb.Dto
{
    public class UserUpdateDto
    {
        public User_Metadata user_metadata { get; set; }

        public class User_Metadata
        {
            public string username { get; set; }
            public string picture { get; set; }
            public string nickname { get; set; }
        }

    }
}
