namespace StatybuWeb.Constants
{
    public static class AzureConstants
    {
        public const string KeyVaultUrl = "https://statybuvault.vault.azure.net/";
        public static class KeyVaultSecretNames
        {
            public const string ConnectionString = "ConnectionString";
            public const string BlobContainer = "PictureBlobStorageContainerName";
        }
    }
}
