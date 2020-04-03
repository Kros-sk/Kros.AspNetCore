using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System;
using System.Linq;
using Kros.Extensions;
using Azure.Identity;

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
        /// <param name="hostingContext">The hosting context.</param>
        /// <remarks>
        /// Configuration should contain attributes AppConfig:Endpoint and AppConfig:Settings.
        /// </remarks>
        public static IConfigurationBuilder AddAzureAppConfiguration(
            this IConfigurationBuilder config,
            HostBuilderContext hostingContext)
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
                        .Select($"{service}:*", hostingContext.HostingEnvironment.EnvironmentName)
                        .Select($"{service}:*")
                        .TrimKeyPrefix($"{service}:");
                }
            });

            return config;
        }
    }
}
