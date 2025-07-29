using Kros.MassTransit.AzureServiceBus;
using MassTransit;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for registering services to the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds MassTransit fluent configurator for Azure service bus.
        /// </summary>
        /// <param name="services">DI container.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="consumerNamespaceAnchor">Type that specifies topmost namespace in which consumers are located.</param>
        /// <param name="busCfg">Service bus configurator.</param>
        /// <returns>MassTransit fluent configuration for Azure service bus.</returns>
        public static IServiceCollection AddMassTransitForAzure(
            this IServiceCollection services,
            IConfiguration configuration,
            Type consumerNamespaceAnchor,
            Action<IMassTransitForAzureBuilder> busCfg = null)
            => AddAzureServiceBus(services, configuration, consumerNamespaceAnchor, (cfg, _) => busCfg?.Invoke(cfg));

        /// <summary>
        /// Adds MassTransit fluent configurator for Azure service bus.
        /// </summary>
        /// <param name="services">DI container.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="consumerNamespaceAnchor">Type that specifies topmost namespace in which consumers are located.</param>
        /// <param name="busCfg">Service bus configurator.</param>
        /// <returns>MassTransit fluent configuration for Azure service bus.</returns>
        public static IServiceCollection AddMassTransitForAzure(
            this IServiceCollection services,
            IConfiguration configuration,
            Type consumerNamespaceAnchor,
            Action<IMassTransitForAzureBuilder, IBusRegistrationContext> busCfg)
            => AddAzureServiceBus(services, configuration, consumerNamespaceAnchor, busCfg);

        /// <summary>
        /// Adds MassTransit fluent configurator for Azure service bus.
        /// </summary>
        /// <param name="services">DI container.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="busCfg">Additional service bus configurator.</param>
        /// <returns>MassTransit fluent configuration for Azure service bus.</returns>
        public static IServiceCollection AddMassTransitForAzure(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IMassTransitForAzureBuilder> busCfg = null)
            => services.AddMassTransitForAzure(configuration, null, busCfg);

        private static IServiceCollection AddAzureServiceBus(
            IServiceCollection services,
            IConfiguration configuration,
            Type consumerNamespaceAnchor,
            Action<IMassTransitForAzureBuilder, IBusRegistrationContext> busCfg)
        {
            const string sectionName = "AzureServiceBus";
            services.Configure<AzureServiceBusOptions>(options => configuration.GetSection(sectionName).Bind(options));

            RegisterConsumers(services, consumerNamespaceAnchor);

            services.AddMassTransit(cfg =>
            {
                if (consumerNamespaceAnchor != null)
                {
                    cfg.AddConsumersFromNamespaceContaining(consumerNamespaceAnchor);
                }
                cfg.AddBus(provider =>
                {
                    MassTransitForAzureBuilder builder = new((IServiceProvider)provider);
                    busCfg?.Invoke(builder, provider);

                    return builder.Build();
                });
            });
            services.AddMassTransitHostedService();

            return services;
        }

        private static void RegisterConsumers(IServiceCollection services, Type namespaceAnchor)
        {
            if (namespaceAnchor != null)
            {
                services.Scan(scan =>
                    scan.FromAssembliesOf(namespaceAnchor)
                    .AddClasses(c => c.AssignableTo(typeof(IConsumer))));
            }
        }
    }
}
