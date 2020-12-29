using GreenPipes.Configurators;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.ConsumeConfigurators;
using System;

namespace Kros.MassTransit.AzureServiceBus
{
    /// <summary>
    /// Builder for fluent configuration of Azure service bus via MassTransit.
    /// </summary>
    public interface IMassTransitForAzureBuilder : IBusEndpointBuilder
    {
        /// <summary>
        /// Configures service bus.
        /// </summary>
        /// <param name="configurator">Delegate to configure service bus.</param>
        /// <returns>Self for further configuring.</returns>
        IMassTransitForAzureBuilder ConfigureServiceBusFactory(Action<IServiceBusBusFactoryConfigurator, IServiceBusHost> configurator = null);
    }

    /// <summary>
    /// Builder for fluent configuration of Azure service bus endpoint consumer.
    /// </summary>
    public interface IBusConsumerBuilder : IBusEndpointBuilder
    {
        /// <summary>
        /// Adds consumer for current endpoint.
        /// </summary>
        /// <typeparam name="TConsumer">Consumer for endpoint.</typeparam>
        /// <param name="configure">Delagate to configure consumer.</param>
        /// <returns>Self for further configuring of endpoints and its consumers.</returns>
        IBusConsumerBuilder AddConsumer<TConsumer>(Action<IConsumerConfigurator<TConsumer>> configure = null)
            where TConsumer : class, IConsumer;

        /// <summary>
        /// Adds consumer for current endpoint.
        /// </summary>
        /// <typeparam name="TMessage">Type of message handled by consumer.</typeparam>
        /// <param name="handler">Delegate to handle message.</param>
        /// <returns>Self for further configuring of endpoints and its consumers.</returns>
        IBusConsumerBuilder AddConsumer<TMessage>(MessageHandler<TMessage> handler) where TMessage : class;
    }

    /// <summary>
    /// Builder for fluent configuration of Azure service bus endpoint.
    /// </summary>
    public interface IBusEndpointBuilder : IBusBuilder
    {
        /// <summary>
        /// Configures queue receive endpoint.
        /// </summary>
        /// <param name="queueName">Name of queue.</param>
        /// <returns>Self for further configuring of endpoints and its consumers.</returns>
        IBusConsumerBuilder ConfigureQueue(string queueName);

        /// <summary>
        /// Configures subscription endpoint.
        /// </summary>
        /// <typeparam name="T">Type of message to be processed.</typeparam>
        /// <param name="subscriptionName">Name of subscription.</param>
        /// <returns>Self for further configuring of endpoints and its consumers.</returns>
        IBusConsumerBuilder ConfigureSubscription<T>(string subscriptionName) where T : class;

        /// <summary>
        /// Configures queue receive endpoint.
        /// </summary>
        /// <param name="queueName">Name of queue.</param>
        /// <param name="configurator">Delegate to configure receive endpoint.</param>
        /// <returns>Self for further configuring of endpoints and its consumers.</returns>
        IBusConsumerBuilder ConfigureQueue(string queueName, Action<IServiceBusReceiveEndpointConfigurator> configurator);

        /// <summary>
        /// Configures subscription endpoint.
        /// </summary>
        /// <typeparam name="T">Type of message to be processed.</typeparam>
        /// <param name="subscriptionName">Name of subscription.</param>
        /// <param name="configurator">Delegate to configure subscription endpoint.</param>
        /// <returns>Self for further configuring of endpoints and its consumers.</returns>
        IBusConsumerBuilder ConfigureSubscription<T>(
            string subscriptionName,
            Action<IServiceBusSubscriptionEndpointConfigurator> configurator) where T : class;

        /// <summary>
        /// Configures message retry using the retry configuration specified.
        /// </summary>
        /// <param name="configure">The retry configuration.</param>
        /// <returns>Self for further configuring of endpoints and its consumers.</returns>
        public IBusConsumerBuilder UseMessageRetry(Action<IRetryConfigurator> configure);
    }

    /// <summary>
    /// Builder for creation of Azure service bus via MassTransit.
    /// </summary>
    public interface IBusBuilder
    {
        /// <summary>
        /// Creates Azure service bus.
        /// </summary>
        /// <returns>Azure service bus.</returns>
        IBusControl Build();
    }
}
