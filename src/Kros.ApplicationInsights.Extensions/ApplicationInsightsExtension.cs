using Kros.ApplicationInsights.Extensions.Options;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kros.ApplicationInsights.Extensions
{
    public static partial class ApplicationInsightsExtension
    {
#pragma warning disable IDE1006 // Naming Styles
        private const string ApplicationInsightsSectionName = "ApplicationInsights";
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Registers application telemetry into DI container.
        /// </summary>
        /// <param name="services">IoC container.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
        {
            ApplicationInsightsOptions options = GetApplicationInsightsOptions(configuration);

            if (options != null)
            {
                services.AddSingleton<ITelemetryInitializer>(new CloudRoleNameInitializer(options.ServiceName));
            }

            return services;
        }

        private static ApplicationInsightsOptions GetApplicationInsightsOptions(IConfiguration configuration)
        {
            IConfigurationSection configurationSection = configuration.GetSection(ApplicationInsightsSectionName);

            if (!configurationSection.Exists())
            {
                return null;
            }

            return configurationSection.Get<ApplicationInsightsOptions>();
        }
    }
}
