using Kros.ApplicationInsights.Extensions;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for simpler use of Application insights.
    /// </summary>
    public static partial class ApplicationInsightsExtension
    {
        private const string ApplicationInsightsSectionName = "ApplicationInsights";

        /// <summary>
        /// Registers application telemetry into DI container.
        /// </summary>
        /// <param name="services">IoC container.</param>
        /// <param name="configuration">Configuration.</param>
        public static IServiceCollection AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
        {
            ApplicationInsightsOptions options = GetApplicationInsightsOptions(configuration);
            services
                .AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
                {
                    EnableAdaptiveSampling = false
                })
                .AddApplicationInsightsTelemetryProcessor<FilterSyntheticRequestsProcessor>()
                .AddApplicationInsightsTelemetryProcessor<FilterRequestsProcessor>();
            if (options != null)
            {
                services.AddSingleton<ITelemetryInitializer>(new CloudRoleNameInitializer(options.ServiceName));
            }
            services.AddSingleton<ITelemetryInitializer, UserIdFromUserAgentInitializer>();
            services.AddSingleton<ITelemetryInitializer, RoutePatternInitializer>();

            return services;
        }

        /// <summary>
        /// Registers application telemetry into DI container.
        /// </summary>
        /// <param name="app">IApplicationBuilder.</param>
        /// <param name="configuration">Configuration.</param>
        public static IApplicationBuilder UseApplicationInsights(this IApplicationBuilder app, IConfiguration configuration)
        {
            TelemetryConfiguration telemetryConfiguration = app.ApplicationServices.GetService<TelemetryConfiguration>();
            TelemetryProcessorChainBuilder builder = telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
            ApplicationInsightsOptions options = GetApplicationInsightsOptions(configuration);

            if (options != null)
            {
                builder.UseSampling(options.SamplingRate);

                if (options.AdaptiveSamplingOptions != null)
                {
                    builder.UseAdaptiveSampling(
                        options.AdaptiveSamplingOptions.MaxTelemetryItemsPerSecond,
                        options.AdaptiveSamplingOptions.ExcludedTypes,
                        options.AdaptiveSamplingOptions.IncludedTypes);
                }
            }

            builder.Build();

            return app;
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
