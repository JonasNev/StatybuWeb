namespace StatybuWeb.Constants
{
    public static class AzureConstants
    {
        public const string KeyVaultUrl = "https://statybuvault.vault.azure.net/";
        public static class KeyVaultSecretNames
        {
            public const string ConnectionString = "ConnectionString";
            public const string BlobContainer = "PictureBlobStorageContainerName";
            public const string Auth0ClientSecret = "Auth0ClientSecret";
            public const string Auth0ClientId = "Auth0ClientId";
            public const string Auth0Domain = "Auth0Domain";
            public const string Auth0ManagementToken10D = "Auth0ManagementToken10D";
        }
    }
}
