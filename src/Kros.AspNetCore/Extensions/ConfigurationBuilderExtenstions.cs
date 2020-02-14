using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using System.Security.Authentication;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Configuration builder extenstions.
    /// </summary>
    public static class ConfigurationBuilderExtenstions
    {
        private const string KeyVaultEndpointConfig = "AzureKeyVault:Endpoint";
        private const string KeyVaultKeyNamePrefixConfig = "AzureKeyVault:KeyNamePrefix";

        /// <summary>
        /// Add local.json configuration.
        /// </summary>
        /// <param name="configurationBuilder">Configuration builder.</param>
        /// <returns>The same instance of the Microsoft.Extensions.Hosting.IHostBuilder for chaining.</returns>
        public static IConfigurationBuilder AddLocalConfiguration(this IConfigurationBuilder configurationBuilder)
            => configurationBuilder.AddJsonFile("appsettings.local.json", optional: true);

        /// <summary>
        /// Adds Azure Key Vault configuration to configuration builder.
        /// </summary>
        /// <param name="config">
        /// Should contain configuration properties AzureKeyVault:Endpoint (url address of azure key vault),
        /// AzureKeyVault:KeyNamePrefix (secrets whose names starts with this prefix will be loaded,
        /// prefix will be removed from the loaded secrets names).
        /// </param>
        public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder config)
        {
            var builtConfig = config.Build();
            var keyVaultEndpoint = builtConfig[KeyVaultEndpointConfig];
            var keyNamePrefix = builtConfig[KeyVaultKeyNamePrefixConfig];

            if (!string.IsNullOrEmpty(keyVaultEndpoint))
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(GetAuthenticationCallback(azureServiceTokenProvider));

                config.AddAzureKeyVault(
                    keyVaultEndpoint,
                    keyVaultClient,
                    new PrefixedKeyVaultSecretManager(keyNamePrefix));
            }

            return config;
        }

        private static KeyVaultClient.AuthenticationCallback GetAuthenticationCallback(
            AzureServiceTokenProvider tokenProvider)
        {
            return async (string authority, string resource, string scope) =>
            {
                try
                {
                    return await tokenProvider.KeyVaultTokenCallback(authority, resource, scope);
                }
                catch (AzureServiceTokenProviderException exception)
                {
                    throw new AuthenticationException("Obtaining an access token for Azure Key Vault failed.", exception);
                }
            };
        }
    }
}
