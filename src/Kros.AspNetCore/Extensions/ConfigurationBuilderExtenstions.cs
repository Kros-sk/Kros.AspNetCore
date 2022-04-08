using Azure.Identity;
using Kros.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Configuration builder extenstions.
    /// </summary>
    public static class ConfigurationBuilderExtenstions
    {
        /// <summary>
        /// Add local.json configuration.
        /// </summary>
        /// <param name="configurationBuilder">Configuration builder.</param>
        /// <returns>The same instance of the Microsoft.Extensions.Hosting.IHostBuilder for chaining.</returns>
        public static IConfigurationBuilder AddLocalConfiguration(this IConfigurationBuilder configurationBuilder)
            => configurationBuilder.AddJsonFile("appsettings.local.json", optional: true);

        /// <summary>
        /// Adds the azure application configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="environmentName">Environment name.</param>
        /// <param name="refreshConfiguration">A callback used to configure Azure App Configuration refresh options.</param>
        /// <returns>The same instance of the Microsoft.Extensions.Hosting.IHostBuilder for chaining.</returns>
        /// <remarks>
        /// Configuration should contain attributes AppConfig:Endpoint and AppConfig:Settings.
        /// </remarks>
        public static IConfigurationBuilder AddAzureAppConfig(
            this IConfigurationBuilder config,
            string environmentName,
            Action<AzureAppConfigurationRefreshOptions> refreshConfiguration = null)
        {
            var settings = config.Build();

            string appConfigEndpoint = settings["AppConfig:Endpoint"];
            if (string.IsNullOrWhiteSpace(appConfigEndpoint))
            {
                return config;
            }

            config.AddAzureAppConfiguration(options =>
            {
                var credential = new DefaultAzureCredential();

                options
                    .Connect(new Uri(settings["AppConfig:Endpoint"]), credential)
                    .ConfigureKeyVault(kv => kv.SetCredential(credential));

                ConfigureCacheRefresh(options, settings, refreshConfiguration);

                IEnumerable<string> services = settings
                    .GetSection("AppConfig:Settings")
                    .AsEnumerable()
                    .Where(p => !p.Value.IsNullOrWhiteSpace())
                    .Select(p => p.Value);

                foreach (string service in services)
                {
                    options
                        .Select($"{service}:*", LabelFilter.Null)
                        .Select($"{service}:*", environmentName)
                        .TrimKeyPrefix($"{service}:");
                }

                string useFeatureFlagsSetting = settings["AppConfig:UseFeatureFlags"];
                if (bool.TryParse(useFeatureFlagsSetting, out bool useFeatureFlags) && useFeatureFlags)
                {
                    options
                        .Select("_", LabelFilter.Null)
                        .Select("_", environmentName)
                        .UseFeatureFlags();
                }
            });

            return config;
        }

        /// <summary>
        /// Adds the azure application configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="hostingContext">The hosting context.</param>
        /// <param name="refreshConfiguration">A callback used to configure Azure App Configuration refresh options.</param>
        /// <returns>The same instance of the Microsoft.Extensions.Hosting.IHostBuilder for chaining.</returns>
        /// <remarks>
        /// Configuration should contain attributes AppConfig:Endpoint and AppConfig:Settings.
        /// </remarks>
        public static IConfigurationBuilder AddAzureAppConfig(
            this IConfigurationBuilder config,
            HostBuilderContext hostingContext,
            Action<AzureAppConfigurationRefreshOptions> refreshConfiguration = null)
            => config.AddAzureAppConfig(hostingContext.HostingEnvironment.EnvironmentName, refreshConfiguration);


        /// <inheritdoc cref="AddAzureAppConfig(IConfigurationBuilder, HostBuilderContext, Action{AzureAppConfigurationRefreshOptions})"/>
        [Obsolete("Use AddAzureAppConfiguration(...) method.")]
        public static IConfigurationBuilder AddAzureAppConfiguration(
            this IConfigurationBuilder config,
            HostBuilderContext hostingContext,
            Action<AzureAppConfigurationRefreshOptions> refreshConfiguration = null)
            => AddAzureAppConfig(config, hostingContext, refreshConfiguration);

        private static void ConfigureCacheRefresh(
            AzureAppConfigurationOptions options,
            IConfigurationRoot settings,
            Action<AzureAppConfigurationRefreshOptions> refreshConfiguration)
        {
            bool sentinelKeySet = !string.IsNullOrWhiteSpace(settings["AppConfig:SentinelKey"]);
            if (sentinelKeySet || refreshConfiguration != null)
            {
                options.ConfigureRefresh(config =>
                {
                    if (sentinelKeySet)
                    {
                        config.Register(settings["AppConfig:SentinelKey"], true);
                    }

                    if (TimeSpan.TryParse(settings["AppConfig:RefreshInterval"], out TimeSpan refreshInterval))
                    {
                        config.SetCacheExpiration(refreshInterval);
                    }

                    refreshConfiguration?.Invoke(config);
                });
            }
        }
    }
}
