using Azure.Extensions.AspNetCore.Configuration.Secrets;
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
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the Azure KeyVault.
        /// </summary>
        /// <param name="configBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="options">Delegate to configure key vault options. The options are preconfigured with values
        /// from section <c>KeyVault</c> in <c>appsettings.json</c> configuration.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAzureKeyVault(
            this IConfigurationBuilder configBuilder,
            Action<KeyVaultOptions> options = null)
        {
            IConfigurationRoot settings = configBuilder.Build();
            KeyVaultOptions kvOptions = new();
            settings.Bind("KeyVault", kvOptions);
            options?.Invoke(kvOptions);

            if (string.IsNullOrWhiteSpace(kvOptions.Name))
            {
                return configBuilder;
            }

            DefaultAzureCredential credential = CreateAzureCredential(kvOptions.IdentityClientId);
            AzureKeyVaultConfigurationOptions kvConfigOptions = new()
            {
                Manager = kvOptions.Prefixes.Count == 0
                    ? new KeyVaultSecretManager()
                    : new PrefixKeyVaultSecretManager(kvOptions.Prefixes),
                ReloadInterval = kvOptions.ReloadInterval > default(TimeSpan) ? kvOptions.ReloadInterval : null
            };

            configBuilder.AddAzureKeyVault(new Uri($"https://{kvOptions.Name}.vault.azure.net/"), credential, kvConfigOptions);

            return configBuilder;
        }

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
        /// <param name="initCredential">A callback used to configure Azure credential.</param>
        /// <returns>The same instance of the Microsoft.Extensions.Hosting.IHostBuilder for chaining.</returns>
        /// <remarks>
        /// Configuration should contain <b>AppConfig</b> section, which is mapped to <see cref="AppConfigOptions "/> class.
        /// </remarks>
        public static IConfigurationBuilder AddAzureAppConfig(
            this IConfigurationBuilder config,
            string environmentName,
            Action<AzureAppConfigurationRefreshOptions> refreshConfiguration = null,
            Action<DefaultAzureCredential> initCredential = null)
        {
            IConfigurationRoot settings = config.Build();
            AppConfigOptions appConfig = new();
            settings.Bind("AppConfig", appConfig);

            if (string.IsNullOrWhiteSpace(appConfig.Endpoint))
            {
                return config;
            }

            config.AddAzureAppConfiguration(options =>
            {
                DefaultAzureCredential credential = CreateAzureCredential(appConfig.IdentityClientId);
                initCredential?.Invoke(credential);

                options
                    .Connect(new Uri(appConfig.Endpoint), credential)
                    .ConfigureKeyVault(kv => kv.SetCredential(credential));

                ConfigureCacheRefresh(options, appConfig, refreshConfiguration);

                IEnumerable<string> prefixes = appConfig.Settings.Where(prefix => !prefix.IsNullOrWhiteSpace());
                foreach (string prefix in prefixes)
                {
                    options
                        .Select($"{prefix}:*", LabelFilter.Null)
                        .Select($"{prefix}:*", environmentName)
                        .TrimKeyPrefix($"{prefix}:");
                }

                if (appConfig.UseFeatureFlags)
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
        /// <param name="initCredential">A callback used to configure Azure credential.</param>
        /// <returns>The same instance of the Microsoft.Extensions.Hosting.IHostBuilder for chaining.</returns>
        /// <remarks>
        /// Configuration should contain <b>AppConfig</b> section, which is mapped to <see cref="AppConfigOptions "/> class.
        /// </remarks>
        public static IConfigurationBuilder AddAzureAppConfig(
            this IConfigurationBuilder config,
            HostBuilderContext hostingContext,
            Action<AzureAppConfigurationRefreshOptions> refreshConfiguration = null,
            Action<DefaultAzureCredential> initCredential = null)
            => config.AddAzureAppConfig(hostingContext.HostingEnvironment.EnvironmentName, refreshConfiguration, initCredential);

        private static void ConfigureCacheRefresh(
            AzureAppConfigurationOptions options,
            AppConfigOptions appConfig,
            Action<AzureAppConfigurationRefreshOptions> refreshConfiguration)
        {
            bool sentinelKeySet = !string.IsNullOrWhiteSpace(appConfig.SentinelKey);
            if (sentinelKeySet || refreshConfiguration != null)
            {
                options.ConfigureRefresh(config =>
                {
                    if (sentinelKeySet)
                    {
                        config.Register(appConfig.SentinelKey, true);
                    }
                    if (appConfig.RefreshInterval > TimeSpan.Zero)
                    {
                        config.SetCacheExpiration(appConfig.RefreshInterval);
                    }
                    refreshConfiguration?.Invoke(config);
                });
            }
        }

        private static DefaultAzureCredential CreateAzureCredential(string managedIdentityClientId)
        {
            DefaultAzureCredentialOptions credentialOptions = new()
            {
                // 'null' means use system assigned identity (if possible). Empty string is invalid value.
                ManagedIdentityClientId = string.IsNullOrEmpty(managedIdentityClientId) ? null : managedIdentityClientId
            };
            ExcludeCredentials(credentialOptions);
            return new DefaultAzureCredential(credentialOptions);
        }

        private static void ExcludeCredentials(DefaultAzureCredentialOptions credentialOptions)
        {
            if (credentialOptions.ManagedIdentityClientId != null)
            {
                credentialOptions.ExcludeEnvironmentCredential = false;
                credentialOptions.ExcludeInteractiveBrowserCredential = false;
                credentialOptions.ExcludeAzurePowerShellCredential = false;
                credentialOptions.ExcludeSharedTokenCacheCredential = false;
                credentialOptions.ExcludeVisualStudioCodeCredential = false;
                credentialOptions.ExcludeVisualStudioCredential = false;
                credentialOptions.ExcludeAzureCliCredential = false;
            }
        }
    }
}
