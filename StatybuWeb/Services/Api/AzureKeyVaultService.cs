using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace StatybuWeb.Services.Api
{
    public class AzureKeyVaultService : IAzureKeyVaultService
    {
        private static readonly Dictionary<string, string> SecretCache = new Dictionary<string, string>();
        private static readonly object CacheLock = new object();
        private readonly SecretClient _keyVaultClient;

        public AzureKeyVaultService()
        {
            var credential = new DefaultAzureCredential();
            _keyVaultClient = new SecretClient(new Uri(Constants.AzureConstants.KeyVaultUrl), credential);
        }

        public async Task<string> GetSecretFromKeyVault(string secretName)
        {
            // Check if the secret exists in the cache
            if (SecretCache.TryGetValue(secretName, out var cachedSecret))
            {
                return cachedSecret;
            }

            // Retrieve the secret from Key Vault outside the lock
            KeyVaultSecret secret = await _keyVaultClient.GetSecretAsync(secretName);

            lock (CacheLock)
            {
                // Double-check within the lock to prevent multiple threads from simultaneously updating the cache
                if (!SecretCache.ContainsKey(secretName))
                {
                    // Cache the retrieved secret
                    SecretCache[secretName] = secret.Value;
                }
            }

            return secret.Value;
        }
    }
}
