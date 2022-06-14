using System;
using System.Collections.Generic;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Settings for Azure App Configuration.
    /// </summary>
    public  class AppConfigOptions
    {
        /// <summary>
        /// Azure App Configuration endpoint URL (https://example.azconfig.io).
        /// </summary>
        public string Endpoint { get; set; } = "";

        /// <summary>
        /// Client ID of the user assigned managed identity, which will be used for connecting to Azure App Configuration.
        /// Leave this empty, if the service is using system assigned managed identity.
        /// </summary>
        public string IdentityClientId { get; set; } = "";

        /// <summary>
        /// List of prefixes that specifies which values will be loaded from App Configuration. Prefix is separated
        /// from the rest of the configuration name with colon <c>:</c>. So prefix itself, cannot contain colon.
        /// In this list, prefixes are set without trailing colon (<c>:</c>).
        /// </summary>
        public List<string> Settings { get; } = new List<string>();

        /// <summary>
        /// Setting to turn on feature flags support.
        /// </summary>
        public bool UseFeatureFlags { get; set; }

        /// <summary>
        /// Key, used for automatic refresh of app configuration.
        /// </summary>
        public string SentinelKey { get; set; } = "";

        /// <summary>
        /// Refresh interval (cache expiration) for the automatically refreshed configuration values.
        /// This must be greater than 1 second.
        /// </summary>
        public TimeSpan RefreshInterval { get; set; }
    }
}
