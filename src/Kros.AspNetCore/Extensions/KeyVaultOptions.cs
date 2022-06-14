using System;
using System.Collections.Generic;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Key vault configuration.
    /// </summary>
    public class KeyVaultOptions
    {
        /// <summary>
        /// Name of the key vault to load configuration from.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Client ID of the user assigned managed identity, which will be used for connecting to key vault.
        /// Leave this empty, if the service is using system assigned managed identity.
        /// </summary>
        public string IdentityClientId { get; set; } = string.Empty;

        /// <summary>
        /// List of prefixes that specifies which secrets will be loaded from key vault. Prefix is separated
        /// from the rest of the secret name with dash <c>-</c>. So prefix itself, cannot contain dash.
        /// In this list, prefixes are set without trailing dash (<c>-</c>).
        /// </summary>
        public List<string> Prefixes { get; } = new();

        /// <summary>
        /// Gets or sets the timespan to wait between attempts at polling the Azure Key Vault for changes.
        /// <code>null</code> to disable reloading.
        /// </summary>
        public TimeSpan? ReloadInterval { get; set; } = null;
    }
}
