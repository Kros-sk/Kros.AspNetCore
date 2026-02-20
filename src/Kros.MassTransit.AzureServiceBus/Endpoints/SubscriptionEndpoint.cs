using MassTransit;
using System;
using System.Collections.Generic;

namespace Kros.MassTransit.AzureServiceBus.Endpoints
{
    /// <summary>
    /// Azure service bus subscription endpoint.
    /// </summary>
    public class SubscriptionEndpoint<TMessage> : Endpoint where TMessage : class
    {
        private readonly Action<IServiceBusSubscriptionEndpointConfigurator> _configurator;
        private readonly List<Action<IServiceBusSubscriptionEndpointConfigurator>> _consumers;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="subscriptionName">Name of subscription.</param>
        /// <param name="configurator">Delegate to configure endpoint.</param>
        public SubscriptionEndpoint(string subscriptionName, Action<IServiceBusSubscriptionEndpointConfigurator> configurator)
            : base(subscriptionName)
        {
            _configurator = configurator;
            _consumers = new List<Action<IServiceBusSubscriptionEndpointConfigurator>>();
        }

        /// <inheritdoc />
        public override void AddConsumer<TMessage2>(MessageHandler<TMessage2> handler)
            => _consumers.Add(endpointConfig => endpointConfig.Handler(handler));

        /// <inheritdoc />
        public override void AddConsumer<TConsumer>(Action<IConsumerConfigurator<TConsumer>> configure = null)
            => _consumers.Add(endpointConfig => endpointConfig.Consumer(() => Activator.CreateInstance<TConsumer>(), configure));

        // We do not want to use Masstransit anymore, so we will not invest in fixing this obsolete warning.
#pragma warning disable CS0618 // Type or member is obsolete
        /// <inheritdoc />
        public override void AddConsumer<TConsumer>(
            IServiceProvider provider,
            Action<IConsumerConfigurator<TConsumer>> configure = null)
            => _consumers.Add(endpointConfig => endpointConfig.Consumer(provider, configure));
#pragma warning restore CS0618 // Type or member is obsolete

        /// <inheritdoc />
        public override void SetEndpoint(IServiceBusBusFactoryConfigurator busCfg)
            => busCfg.SubscriptionEndpoint<TMessage>(Name, endpointConfig =>
            {
                SetDefaults(endpointConfig);

                _configurator?.Invoke(endpointConfig);

                foreach (Action<IServiceBusSubscriptionEndpointConfigurator> consumerConfig in _consumers)
                {
                    consumerConfig?.Invoke(endpointConfig);
                }
            });
    }
}
