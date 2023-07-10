namespace StatybuWeb.Models.Auth0
{
    public class User
    {
        public string? Email { get; set; }
        public bool? Email_verified { get; set; }
        public string? Username { get; set; }
        public string? Phone_number { get; set; }
        public bool? Phone_verified { get; set; }
        public string? User_id { get; set; }
        public string? Created_at { get; set; }
        public string? Updated_at { get; set; }
        public Identity[]? Identities { get; set; }
        public App_Metadata? App_metadata { get; set; }
        public User_Metadata? User_metadata { get; set; }
        public string? Picture { get; set; }
        public string? Name { get; set; }
        public string? Nickname { get; set; }
        public string[]? Multifactor { get; set; }
        public string? Last_ip { get; set; }
        public string? Last_login { get; set; }
        public int? Logins_count { get; set; }
        public bool? blocked { get; set; }
        public string? Given_name { get; set; }
        public string? Family_name { get; set; }

        public class App_Metadata
        {
        }

        public class User_Metadata
        {
            public string? Username { get; set; }
            public string? Picture { get; set; }
            public string? Nickname { get; set; }
        }

        public class Identity
        {
            public string? Connection { get; set; }
            public string? User_id { get; set; }
            public string? Provider { get; set; }
            public bool IsSocial { get; set; }
        }

    }
}
