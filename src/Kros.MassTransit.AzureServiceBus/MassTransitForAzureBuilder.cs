using Azure;
using Azure.Messaging.ServiceBus;
using Kros.MassTransit.AzureServiceBus.Endpoints;
using Kros.Utils;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Kros.MassTransit.AzureServiceBus
{
    /// <summary>
    /// Builder for Azure service bus via MassTransit.
    /// </summary>
    public class MassTransitForAzureBuilder : IMassTransitForAzureBuilder, IBusConsumerBuilder
    {
        #region Attributes

        private readonly string _connectionString;
        private readonly IServiceProvider _provider;
        private readonly string _topicNamePrefix;
        private readonly string _endpointNamePrefix;
        private Action<IServiceBusBusFactoryConfigurator> _busConfigurator;
        private readonly List<Endpoint> _endpoints = new();
        private Endpoint _currentEndpoint;
        private Action<IRetryConfigurator> _retryConfigurator;

        #endregion

        #region Constructors

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="connectionString">Connection string to Azure service bus.</param>
        public MassTransitForAzureBuilder(string connectionString) : this(connectionString, null)
        { }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="connectionString">Connection string to Azure service bus.</param>
        /// <param name="tokenTimeToLive">TTL for Azure service bus token.</param>
        [Obsolete("'tokenTimeToLive' parameter is not used anymore. Use constructor without this parameter.")]
        public MassTransitForAzureBuilder(string connectionString, TimeSpan tokenTimeToLive) :
            this(connectionString, null)
        { }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="connectionString">Connection string to Azure service bus.</param>
        /// <param name="provider">DI container.</param>
        public MassTransitForAzureBuilder(string connectionString, IServiceProvider provider)
        {
            _connectionString = Check.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            _provider = provider;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="provider">DI container.</param>
        public MassTransitForAzureBuilder(IServiceProvider provider)
        {
            AzureServiceBusOptions options = provider.GetService<IOptions<AzureServiceBusOptions>>().Value;

            _connectionString = Check.NotNullOrWhiteSpace(options.ConnectionString,
                nameof(AzureServiceBusOptions.ConnectionString));
            _provider = provider;
            _topicNamePrefix = options.TopicNamePrefix;
            _endpointNamePrefix = options.EndpointNamePrefix;
            _retryConfigurator = CreateDefaultRetryConfigurator(options);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="registrationContext">MassTransit registration context.</param>
        [Obsolete("'registrationContext' is not used anymore. Use constructor with IServiceProvider paramter.")]
        public MassTransitForAzureBuilder(IBusRegistrationContext registrationContext)
            : this((IServiceProvider)registrationContext)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="connectionString">Connection string to Azure service bus.</param>
        /// <param name="tokenTimeToLive">TTL for Azure service bus token.</param>
        /// <param name="provider">DI container.</param>
        [Obsolete("'tokenTimeToLive' parameter is not used anymore. Use constructor without this parameter.")]
        public MassTransitForAzureBuilder(string connectionString, TimeSpan tokenTimeToLive, IServiceProvider provider)
            : this(connectionString, provider)
        {
        }

        #endregion

        #region Config

        /// <inheritdoc />
        public IMassTransitForAzureBuilder ConfigureServiceBusFactory(
            Action<IServiceBusBusFactoryConfigurator> configurator = null)
        {
            _busConfigurator = configurator;
            return this;
        }

        #endregion

        #region Endpoints

        /// <inheritdoc />
        public IBusConsumerBuilder ConfigureQueue(string queueName, Action<IServiceBusReceiveEndpointConfigurator> configurator)
        {
            _currentEndpoint = new ReceiveEndpoint(PrefixEndpointName(queueName), configurator);
            _endpoints.Add(_currentEndpoint);

            return this;
        }

        /// <inheritdoc />
        public IBusConsumerBuilder ConfigureSubscription<T>(
            string subscriptionName,
            Action<IServiceBusSubscriptionEndpointConfigurator> configurator) where T : class
        {
            _currentEndpoint = new SubscriptionEndpoint<T>(PrefixEndpointName(subscriptionName), configurator);
            _endpoints.Add(_currentEndpoint);

            return this;
        }

        /// <inheritdoc />
        public IBusConsumerBuilder ConfigureQueue(string queueName)
            => ConfigureQueue(queueName, config => { });

        /// <inheritdoc />
        public IBusConsumerBuilder ConfigureSubscription<T>(string subscriptionName) where T : class
            => ConfigureSubscription<T>(subscriptionName, config => { });

        #endregion

        #region Consumers

        /// <inheritdoc />
        public IBusConsumerBuilder AddConsumer<TConsumer>(Action<IConsumerConfigurator<TConsumer>> configure = null)
            where TConsumer : class, IConsumer
        {
            if (typeof(IConsumer).GetConstructor(Type.EmptyTypes) == null)
            {
                Check.NotNull(_provider, nameof(_provider));
                _currentEndpoint.AddConsumer(_provider, configure);
            }
            else
            {
                _currentEndpoint.AddConsumer(configure);
            }

            return this;
        }

        /// <inheritdoc />
        public IBusConsumerBuilder AddConsumer<T>(MessageHandler<T> handler) where T : class
        {
            _currentEndpoint.AddConsumer(handler);
            return this;
        }

        #endregion

        #region Retrying

        /// <inheritdoc/>
        public IBusConsumerBuilder UseMessageRetry(Action<IRetryConfigurator> configure)
        {
            _retryConfigurator = configure;
            return this;
        }

        private Action<IRetryConfigurator> CreateDefaultRetryConfigurator(AzureServiceBusOptions options)
        {
            if (options.IntervalRetry?.Limit > 0)
            {
                int limit = options.IntervalRetry.Limit;
                int interval = options.IntervalRetry.Interval;

                return new Action<IRetryConfigurator>(retry =>
                    retry.Interval(limit, interval));
            }
            return null;
        }

        #endregion

        #region Build

        /// <inheritdoc />
        public IBusControl Build()
        {
            IBusControl bus = Bus.Factory.CreateUsingAzureServiceBus(busCfg =>
            {
                CreateServiceHost(busCfg);

                ConfigureServiceBus(busCfg);
                AddMessageTypePrefix(busCfg);
                AddEndpoints(busCfg);

                if (_retryConfigurator != null)
                {
                    busCfg.UseMessageRetry(_retryConfigurator);
                }
            });

            return bus;
        }

        /// <summary>
        /// Creates service bus host.
        /// </summary>
        /// <param name="busCfg">Service bus configuration.</param>
        /// <returns>Service bus host.</returns>
        private void CreateServiceHost(IServiceBusBusFactoryConfigurator busCfg)
            => busCfg.Host(_connectionString);

        /// <summary>
        /// Configures service bus.
        /// </summary>
        /// <param name="busCfg">Service bus configuration.</param>
        private void ConfigureServiceBus(IServiceBusBusFactoryConfigurator busCfg)
        {
            busCfg.UseJsonSerializer();
            busCfg.DefaultMessageTimeToLive = ConfigDefaults.MessageTimeToLive;
            busCfg.EnableDeadLetteringOnMessageExpiration = ConfigDefaults.EnableDeadLetteringOnMessageExpiration;
            busCfg.LockDuration = ConfigDefaults.LockDuration;
            busCfg.AutoDeleteOnIdle = ConfigDefaults.AutoDeleteOnIdle;
            busCfg.MaxDeliveryCount = ConfigDefaults.MaxDeliveryCount;
            busCfg.EnableDuplicateDetection(ConfigDefaults.DuplicateDetectionWindow);

            _busConfigurator?.Invoke(busCfg);
        }

        /// <summary>
        /// Adds endpoints to service bus.
        /// </summary>
        /// <param name="busCfg">Service bus configuration.</param>
        private void AddEndpoints(IServiceBusBusFactoryConfigurator busCfg)
        {
            foreach (Endpoint endpoint in _endpoints)
            {
                endpoint.SetEndpoint(busCfg);
            }
        }

        private void AddMessageTypePrefix(IServiceBusBusFactoryConfigurator configurator)
        {
            if (!string.IsNullOrWhiteSpace(_topicNamePrefix))
            {
                configurator.MessageTopology.SetEntityNameFormatter(
                    new PrefixEntityNameFormatter(configurator.MessageTopology.EntityNameFormatter, _topicNamePrefix));
            }
        }

        private string PrefixEndpointName(string queueName) =>
            string.IsNullOrEmpty(_endpointNamePrefix) ? queueName : $"{_endpointNamePrefix}{queueName}";

        #endregion
    }
}
