namespace StatybuWeb.Models.Auth0
{
    public class User
    {
        public string email { get; set; }
        public bool email_verified { get; set; }
        public string username { get; set; }
        public string phone_number { get; set; }
        public bool phone_verified { get; set; }
        public string user_id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public Identity[] identities { get; set; }
        public App_Metadata app_metadata { get; set; }
        public User_Metadata user_metadata { get; set; }
        public string picture { get; set; }
        public string name { get; set; }
        public string nickname { get; set; }
        public string[] multifactor { get; set; }
        public string last_ip { get; set; }
        public string last_login { get; set; }
        public int logins_count { get; set; }
        public bool blocked { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }

        public class App_Metadata
        {
        }

        public class User_Metadata
        {
            public string username { get; set; }
            public string picture { get; set; }
            public string nickname { get; set; }
        }

        public class Identity
        {
            public string connection { get; set; }
            public string user_id { get; set; }
            public string provider { get; set; }
            public bool isSocial { get; set; }
        }

    }
}
