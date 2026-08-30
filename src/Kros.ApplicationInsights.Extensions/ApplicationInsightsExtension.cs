using Kros.ApplicationInsights.Extensions;
using Kros.Utils;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Options;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

            services.AddApplicationInsightsTelemetry(serviceOptions => ConfigureSampling(serviceOptions, options));
            services.AddHttpContextAccessor();

            IOpenTelemetryBuilder builder = services.AddOpenTelemetry();

            if (!string.IsNullOrEmpty(options?.ServiceName))
            {
                // Cloud role name and role instance are derived from the OpenTelemetry resource.
                builder.ConfigureResource(resource
                    => resource.AddService(options.ServiceName, serviceInstanceId: options.ServiceName));
            }

            builder.WithTracing(tracing => tracing
                .AddProcessor<FilterSyntheticRequestsProcessor>()
                .AddProcessor<FilterRequestsProcessor>()
                .AddProcessor<UserIdFromUserAgentProcessor>()
                .AddProcessor<RoutePatternProcessor>());

            return services;
        }

        /// <summary>
        /// Registers application telemetry into DI container.
        /// </summary>
        /// <param name="app">IApplicationBuilder.</param>
        /// <param name="configuration">Configuration.</param>
        [Obsolete("Sampling is configured during service registration in " + nameof(AddApplicationInsights)
            + ". This method does nothing and will be removed in a future version.")]
        public static IApplicationBuilder UseApplicationInsights(this IApplicationBuilder app, IConfiguration configuration)
            => app;

        /// <summary>
        /// Adds the processor which redacts log records containing sensitive data.
        /// </summary>
        /// <param name="services">The services.</param>
        public static IServiceCollection AddSensitiveTraceTelemetryProcessor(this IServiceCollection services)
        {
            services.AddOpenTelemetry()
                .WithLogging(logging => logging.AddProcessor<FilterSensitiveTraceTelemetryProcessor>());

            return services;
        }

        /// <summary>
        /// Adds the headers telemetry processor.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="propertyNameResolver">The property name resolver.</param>
        /// <param name="headersToCapture">The headers to capture.</param>
        public static IServiceCollection AddHeadersTelemetryProcessor(
            this IServiceCollection services,
            Func<string, string> propertyNameResolver = null,
            params string[] headersToCapture)
        {
            Check.GreaterThan(headersToCapture.Length, 0, nameof(headersToCapture));

            services.AddHttpContextAccessor();
            services.Configure<HeadersTelemetryProcessor.HeadersToCaptureOptions>(options =>
            {
                options.Add(headersToCapture);
                if (propertyNameResolver is not null)
                {
                    options.PropertyNameResolver = propertyNameResolver;
                }
            });
            services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddProcessor<HeadersTelemetryProcessor>());

            return services;
        }

        /// <summary>
        /// Adds the headers telemetry processor.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="headersToCapture">The headers to capture.</param>
        public static IServiceCollection AddHeadersTelemetryProcessor(
            this IServiceCollection services,
            params string[] headersToCapture)
            => services.AddHeadersTelemetryProcessor(null, headersToCapture);

        /// <summary>
        /// Adds the headers telemetry initializer.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="propertyNameResolver">The property name resolver.</param>
        /// <param name="headersToCapture">The headers to capture.</param>
        [Obsolete($"Use {nameof(AddHeadersTelemetryProcessor)}.", error: true)]
        public static IServiceCollection AddHeadersTelemetryInitializer(
            this IServiceCollection services,
            Func<string, string> propertyNameResolver = null,
            params string[] headersToCapture)
            => AddHeadersTelemetryProcessor(services, propertyNameResolver, headersToCapture);

        /// <summary>
        /// Adds the headers telemetry initializer.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="headersToCapture">The headers to capture.</param>
        [Obsolete($"Use {nameof(AddHeadersTelemetryProcessor)}.", error: true)]
        public static IServiceCollection AddHeadersTelemetryInitializer(
            this IServiceCollection services,
            params string[] headersToCapture)
            => services.AddHeadersTelemetryInitializer(null, headersToCapture);

        private static void ConfigureSampling(
            ApplicationInsightsServiceOptions serviceOptions,
            ApplicationInsightsOptions options)
        {
            if (options is null)
            {
                return;
            }

            if (options.AdaptiveSamplingOptions is not null)
            {
                // Rate limited sampling. It replaces adaptive sampling known from the previous SDK version.
                serviceOptions.TracesPerSecond = options.AdaptiveSamplingOptions.MaxTelemetryItemsPerSecond;
            }
            else if (options.SamplingRate > 0)
            {
                // Fixed rate sampling. Sampling rate is a percentage, sampling ratio is a number from 0 to 1.
                serviceOptions.SamplingRatio = options.SamplingRate / 100f;
            }
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
