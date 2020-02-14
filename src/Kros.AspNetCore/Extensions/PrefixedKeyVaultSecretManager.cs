using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Key vault secret manager with remove key name prefix functionality.
    /// </summary>
    public class PrefixedKeyVaultSecretManager : DefaultKeyVaultSecretManager
    {
        private readonly string _keyNamePrefix;
        private const char ConfigurationNamesSeparator = ':';
        private const char KeyVaultNamesSeparator = '-';
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="keyNamePrefix">
        /// Prefix of key, secret or certificate name.
        /// All trailing dash '-' or colon ':' characters will be removed;
        /// </param>
        public PrefixedKeyVaultSecretManager(string keyNamePrefix)
        {
            _keyNamePrefix = keyNamePrefix.TrimEnd(KeyVaultNamesSeparator, ConfigurationNamesSeparator);
        }

        /// <summary>
        /// Replaces -- with : . Removes key name prefix
        /// (provided as constructor argument <see cref="PrefixedKeyVaultSecretManager"/>)
        /// and subsequently all leading colons ':' from the start of the key (or secret or certificate) name.
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public override string GetKey(SecretBundle secret)
        {
            // replaces -- with :
            var configKey = base.GetKey(secret);

            if (configKey.StartsWith(_keyNamePrefix))
            {
                configKey = configKey.Remove(0, _keyNamePrefix.Length);
            }

            configKey = configKey.TrimStart(ConfigurationNamesSeparator);

            return configKey;
        }

        /// <summary>
        /// Determines if secret value should be loaded. Returns true for secret values whose name starts with key name prefix
        /// (provided as constructor argument <see cref="PrefixedKeyVaultSecretManager"/>).
        /// </summary>
        /// <param name="secret"></param>
        /// <returns>true if secret value should be loaded</returns>
        public override bool Load(SecretItem secret)
        {
            return secret.Identifier.Name.StartsWith(_keyNamePrefix);
        }
    }
}
