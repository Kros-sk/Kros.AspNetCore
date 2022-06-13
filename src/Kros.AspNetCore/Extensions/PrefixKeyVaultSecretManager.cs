using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Kros.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Implementation of <see cref="KeyVaultSecretManager"/> which loads only secrets with defined prefix(es).
    /// Prefixes are set without trailing dash (<c>-</c>), but secret names must contain it. For example prefix
    /// <c>Lorem</c> will load all secrets which name starts with <c>Lorem-</c>.
    /// </summary>
    public class PrefixKeyVaultSecretManager : KeyVaultSecretManager
    {
        private readonly HashSet<string> _prefixes = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates an instance with specified <paramref name="prefix"/>.
        /// </summary>
        /// <param name="prefix">Prefix for key vault secrets.</param>
        /// <exception cref="ArgumentNullException">
        /// The value of <paramref name="prefix"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The value of <paramref name="prefix"/> is empty string, or string containing only white-space characters.
        /// </exception>
        public PrefixKeyVaultSecretManager(string prefix)
        {
            Check.NotNullOrWhiteSpace(prefix, nameof(prefix));
            AddPrefix(prefix);
        }

        /// <summary>
        /// Creates an instance with specified <paramref name="prefixes"/>.
        /// </summary>
        /// <param name="prefixes"></param>
        /// <exception cref="ArgumentNullException">
        /// The value of <paramref name="prefixes"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="prefixes"/> is an ampty list, or does not contain any valid
        /// prefix. Prefix must not be an empty string or a string containing only white-space characters.
        /// </exception>
        public PrefixKeyVaultSecretManager(IEnumerable<string> prefixes)
        {
            Check.NotNull(prefixes, nameof(prefixes));
            foreach (string prefix in prefixes)
            {
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    AddPrefix(prefix);
                }
            }
            if (_prefixes.Count == 0)
            {
                throw new ArgumentException("At least one valid prefix must be supplied. Prefix cannot be an empty string " +
                    "or consists only of white-space characters.", nameof(prefixes));
            }
        }

        private void AddPrefix(string prefix) => _prefixes.Add(prefix + "-");

        /// <summary>
        /// Checks if <see cref="KeyVaultSecret"/> value should be retrieved. The secret is retrieved, if its name
        /// has one of the defined prefixes.
        /// </summary>
        /// <param name="secret">The <see cref="SecretProperties"/> instance.</param>
        /// <returns><code>true</code> if secrets value should be loaded, otherwise <code>false</code>.</returns>
        public override bool Load(SecretProperties secret)
            => _prefixes.Any(prefix => secret.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Maps secret to a configuration key. Prefix is removed from secret name and double dashes (<c>--</c>)
        /// are replaced with colon (<c>:</c>).
        /// </summary>
        /// <param name="secret">The <see cref="KeyVaultSecret"/> instance.</param>
        /// <returns>Configuration key name to store secret value.</returns>
        public override string GetKey(KeyVaultSecret secret)
        {
            int keyStart = secret.Name.IndexOf('-') + 1;
            return secret.Name[keyStart..].Replace("--", ConfigurationPath.KeyDelimiter);
        }
    }
}
