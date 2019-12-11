using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.ConsumeConfigurators;
using System;
using System.Collections.Generic;

namespace Kros.MassTransit.AzureServiceBus.Endpoints
{
    /// <summary>
    /// Azure service bus receive endpoint.
    /// </summary>
    public class ReceiveEndpoint : Endpoint
    {
        private readonly Action<IServiceBusReceiveEndpointConfigurator> _configurator;
        private readonly List<Action<IServiceBusReceiveEndpointConfigurator>> _consumers;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="queueName">Name of queue.</param>
        /// <param name="configurator">Delegate to configure endpoint.</param>
        public ReceiveEndpoint(string queueName, Action<IServiceBusReceiveEndpointConfigurator> configurator)
            : base(queueName)
        {
            _configurator = configurator;
            _consumers = new List<Action<IServiceBusReceiveEndpointConfigurator>>();
        }

        /// <inheritdoc />
        public override void AddConsumer<TMessage>(MessageHandler<TMessage> handler)
            => _consumers.Add(endpointConfig => endpointConfig.Handler(handler));

        /// <inheritdoc />
        public override void AddConsumer<TConsumer>(Action<IConsumerConfigurator<TConsumer>> configure = null)
            => _consumers.Add(endpointConfig => endpointConfig.Consumer(() => Activator.CreateInstance<TConsumer>(), configure));

        /// <inheritdoc />
        public override void AddConsumer<TConsumer>(
            IServiceProvider provider,
            Action<IConsumerConfigurator<TConsumer>> configure = null)
            => _consumers.Add(endpointConfig => endpointConfig.Consumer(provider, configure));

        /// <inheritdoc />
        public override void SetEndpoint(IServiceBusBusFactoryConfigurator busCfg)
            => busCfg.ReceiveEndpoint(Name, endpointConfig =>
            {
                _configurator?.Invoke(endpointConfig);

                foreach (Action<IServiceBusReceiveEndpointConfigurator> consumerConfig in _consumers)
                {
                    consumerConfig?.Invoke(endpointConfig);
                }
            });
    }
}
