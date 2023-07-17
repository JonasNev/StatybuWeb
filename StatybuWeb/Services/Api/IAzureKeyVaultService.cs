namespace StatybuWeb.Services.Api
{
    public interface IAzureKeyVaultService
    {
        Task<string> GetSecretFromKeyVault(string secretName);
    }
}
