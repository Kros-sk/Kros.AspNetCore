using Kros.Utils;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.ConsumeConfigurators;
using System;

namespace Kros.MassTransit.AzureServiceBus.Endpoints
{
    /// <summary>
    /// Class representing Azure service bus endpoint.
    /// </summary>
    public abstract class Endpoint
    {
        /// <summary>
        /// Initialize new instance with specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the endpoint.</param>
        protected Endpoint(string name)
        {
            Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        }

        /// <summary>
        /// Endpoint name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Adds new consumer to endpoint.
        /// </summary>
        /// <typeparam name="TMessage">Type of message processed by consumer.</typeparam>
        /// <param name="handler">Delegate to process message.</param>
        public abstract void AddConsumer<TMessage>(MessageHandler<TMessage> handler) where TMessage : class;

        /// <summary>
        /// Adds new consumer to endpoint.
        /// </summary>
        /// <typeparam name="TConsumer">Type of message consumer.</typeparam>
        /// <param name="configure">Delegate to configure consumer.</param>
        public abstract void AddConsumer<TConsumer>(Action<IConsumerConfigurator<TConsumer>> configure = null)
            where TConsumer : class, IConsumer;

        /// <summary>
        /// Adds new consumer with dependencies to endpoint.
        /// </summary>
        /// <typeparam name="TConsumer">Type of message consumer.</typeparam>
        /// <param name="provider">Service provider (DI container).</param>
        /// <param name="configure">Delegate to configure consumer.</param>
        public abstract void AddConsumer<TConsumer>(
            IServiceProvider provider,
            Action<IConsumerConfigurator<TConsumer>> configure = null) where TConsumer : class, IConsumer;

        /// <summary>
        /// Sets endpoint and its consumers during service bus initialization.
        /// </summary>
        /// <param name="busCfg">Service bus configuration.</param>
        /// <param name="host">Service bus host.</param>
        public abstract void SetEndpoint(IServiceBusBusFactoryConfigurator busCfg, IServiceBusHost host);
    }
}
