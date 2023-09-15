using Kros.ApplicationInsights.Extensions;
using Kros.Utils;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Options;
using System;

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

        /// <summary>
        /// Adds the headers telemetry initializer.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="propertyNameResolver">The property name resolver.</param>
        /// <param name="headersToCapture">The headers to capture.</param>
        public static IServiceCollection AddHeadersTelemetryInitializer(
            this IServiceCollection services,
            Func<string, string> propertyNameResolver = null,
            params string[] headersToCapture)
        {
            Check.GreaterThan(headersToCapture.Length, 0, nameof(headersToCapture));

            services.AddHttpContextAccessor();
            services.Configure<HeadersTelemetryInitializer.HeadersToCaptureOptions>(options =>
            {
                options.Add(headersToCapture);
                if (propertyNameResolver is not null)
                {
                    options.PropertyNameResolver = propertyNameResolver;
                }
            });
            services.AddSingleton<ITelemetryInitializer, HeadersTelemetryInitializer>();
            return services;
        }

        /// <summary>
        /// Adds the headers telemetry initializer.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="headersToCapture">The headers to capture.</param>
        public static IServiceCollection AddHeadersTelemetryInitializer(
            this IServiceCollection services,
            params string[] headersToCapture)
            => services.AddHeadersTelemetryInitializer(null, headersToCapture);
    }
}
