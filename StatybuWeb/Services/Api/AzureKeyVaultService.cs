using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using StatybuWeb.Constants;
using System.Collections.Concurrent;
using Microsoft.VisualBasic;
using StatybuWeb.Services.Api;

public class AzureKeyVaultService : IAzureKeyVaultService
{
    private static readonly ConcurrentDictionary<string, string> SecretCache = new ConcurrentDictionary<string, string>();
    private readonly SecretClient _keyVaultClient;
    private readonly ILogger<AzureKeyVaultService> _logger;

    public AzureKeyVaultService(ILogger<AzureKeyVaultService> logger)
    {
        var credential = new DefaultAzureCredential();
        _keyVaultClient = new SecretClient(new Uri(AzureConstants.KeyVaultUrl), credential);
        _logger = logger;
    }

    public async Task<string> GetSecretFromKeyVault(string secretName)
    {
        if (string.IsNullOrEmpty(secretName))
        {
            _logger.LogError("Secret name is null or empty");
            throw new ArgumentNullException(nameof(secretName));
        }

        if (SecretCache.TryGetValue(secretName, out var cachedSecret))
        {
            _logger.LogInformation($"Retrieved secret {secretName} from cache");
            return cachedSecret;
        }

        KeyVaultSecret secret = await _keyVaultClient.GetSecretAsync(secretName);

        if (secret == null)
        {
            _logger.LogError($"Failed to retrieve secret {secretName} from Key Vault");
            throw new Exception($"Secret {secretName} not found in Key Vault");
        }

        SecretCache[secretName] = secret.Value;

        _logger.LogInformation($"Retrieved secret {secretName} from Key Vault and cached it");
        return secret.Value;
    }
}
