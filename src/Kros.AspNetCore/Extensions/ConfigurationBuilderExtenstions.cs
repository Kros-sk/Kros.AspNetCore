using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System;
using System.Linq;
using Kros.Extensions;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

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
        /// <remarks>
        /// Configuration should contain attributes AppConfig:Endpoint and AppConfig:Settings.
        /// </remarks>
        public static IConfigurationBuilder AddAzureAppConfig(
            this IConfigurationBuilder config, string environmentName)
        {
            var settings = config.Build();

            config.AddAzureAppConfiguration(options =>
            {
                var credential = new DefaultAzureCredential();

                options
                    .Connect(new Uri(settings["AppConfig:Endpoint"]), credential)
                    .ConfigureKeyVault(kv => kv.SetCredential(credential));

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
        /// <remarks>
        /// Configuration should contain attributes AppConfig:Endpoint and AppConfig:Settings.
        /// </remarks>
        public static IConfigurationBuilder AddAzureAppConfiguration(
            this IConfigurationBuilder config,
            HostBuilderContext hostingContext) => config.AddAzureAppConfig(hostingContext.HostingEnvironment.EnvironmentName);

        /// <summary>
        /// Adds the azure application configuration if AppConfig:Endpoint is defined.
        /// Does not throw exception if AppConfig:Endpoint is not defined.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="environmentName">Environment name.</param>
        /// <remarks>
        /// Configuration should contain attributes AppConfig:Endpoint and AppConfig:Settings.
        /// </remarks>
        public static IConfigurationBuilder AddAzureAppConfigurationIfEndpointDefined(
            this IConfigurationBuilder config,
            string environmentName)
        {
            var settings = config.Build();
            string appConfigEndpoint = settings["AppConfig:Endpoint"];
            if (!string.IsNullOrWhiteSpace(appConfigEndpoint))
            {
                config.AddAzureAppConfig(environmentName);
            }
            return config;
        }
    }
}
