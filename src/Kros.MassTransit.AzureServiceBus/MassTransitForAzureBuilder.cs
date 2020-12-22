using Kros.MassTransit.AzureServiceBus.Endpoints;
using Kros.Utils;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.ConsumeConfigurators;
using Microsoft.Azure.ServiceBus;
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
        private readonly TimeSpan _tokenTimeToLive;
        private readonly IRegistrationContext<IServiceProvider> _registrationContext;
        private readonly IServiceProvider _provider;
        private readonly string _topicNamePrefix;
        private Action<IServiceBusBusFactoryConfigurator, IServiceBusHost> _busConfigurator;
        private readonly List<Endpoint> _endpoints = new List<Endpoint>();
        private Endpoint _currentEndpoint;

        #endregion

        #region Constructors

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="connectionString">Connection string to Azure service bus.</param>
        public MassTransitForAzureBuilder(string connectionString) : this(connectionString, ConfigDefaults.TokenTimeToLive)
        { }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="connectionString">Connection string to Azure service bus.</param>
        /// <param name="tokenTimeToLive">TTL for Azure service bus token.</param>
        public MassTransitForAzureBuilder(string connectionString, TimeSpan tokenTimeToLive) :
            this(connectionString, tokenTimeToLive, null)
        { }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="connectionString">Connection string to Azure service bus.</param>
        /// <param name="provider">DI container.</param>
        public MassTransitForAzureBuilder(string connectionString, IServiceProvider provider) :
            this(connectionString, ConfigDefaults.TokenTimeToLive, provider)
        { }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="provider">DI container.</param>
        public MassTransitForAzureBuilder(IServiceProvider provider)
        {
            AzureServiceBusOptions options = provider.GetService<IOptions<AzureServiceBusOptions>>().Value;

            _connectionString = Check.NotNullOrWhiteSpace(options.ConnectionString,
                nameof(AzureServiceBusOptions.ConnectionString));
            _tokenTimeToLive = options.TokenTimeToLive > 0
                ? TimeSpan.FromSeconds(options.TokenTimeToLive)
                : ConfigDefaults.TokenTimeToLive;
            _provider = provider;
            _topicNamePrefix = options.TopicNamePrefix;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="registrationContext">MassTransit registration context.</param>
        public MassTransitForAzureBuilder(IRegistrationContext<IServiceProvider> registrationContext)
            : this(registrationContext.Container)
        {
            _registrationContext = registrationContext;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="connectionString">Connection string to Azure service bus.</param>
        /// <param name="tokenTimeToLive">TTL for Azure service bus token.</param>
        /// <param name="provider">DI container.</param>
        public MassTransitForAzureBuilder(string connectionString, TimeSpan tokenTimeToLive, IServiceProvider provider)
        {
            _connectionString = Check.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            _tokenTimeToLive = Check.GreaterThan(tokenTimeToLive, TimeSpan.Zero, nameof(tokenTimeToLive));
            _provider = provider;
        }

        #endregion

        #region Config

        /// <inheritdoc />
        public IMassTransitForAzureBuilder ConfigureServiceBusFactory(
            Action<IServiceBusBusFactoryConfigurator, IServiceBusHost> configurator = null)
        {
            _busConfigurator = configurator;
            return this;
        }

        #endregion

        #region Endpoints

        /// <inheritdoc />
        public IBusConsumerBuilder ConfigureQueue(string queueName, Action<IServiceBusReceiveEndpointConfigurator> configurator)
        {
            _currentEndpoint = new ReceiveEndpoint(queueName, configurator);
            _endpoints.Add(_currentEndpoint);

            return this;
        }

        /// <inheritdoc />
        public IBusConsumerBuilder ConfigureSubscription<T>(
            string subscriptionName,
            Action<IServiceBusSubscriptionEndpointConfigurator> configurator) where T : class
        {
            _currentEndpoint = new SubscriptionEndpoint<T>(subscriptionName, configurator);
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

        #region Build

        /// <inheritdoc />
        public IBusControl Build()
        {
            IBusControl bus = Bus.Factory.CreateUsingAzureServiceBus(busCfg =>
            {
                IServiceBusHost host = CreateServiceHost(busCfg);

                ConfigureServiceBus(busCfg, host);
                AddMessageTypePrefix(busCfg);
                AddEndpoints(busCfg);

                if (_registrationContext != null)
                {
                    busCfg.UseHealthCheck(_registrationContext);
                }
            });

            return bus;
        }

        /// <summary>
        /// Creates service bus host.
        /// </summary>
        /// <param name="busCfg">Service bus configuration.</param>
        /// <returns>Service bus host.</returns>
        private IServiceBusHost CreateServiceHost(IServiceBusBusFactoryConfigurator busCfg)
        {
            var cstrBuilder = new ServiceBusConnectionStringBuilder(_connectionString);

            return busCfg.Host(_connectionString, hostCfg =>
            {
                hostCfg.SharedAccessSignature(sasCfg =>
                {
                    sasCfg.KeyName = cstrBuilder.SasKeyName;
                    sasCfg.SharedAccessKey = cstrBuilder.SasKey;
                    sasCfg.TokenTimeToLive = _tokenTimeToLive;
                });
            });
        }

        /// <summary>
        /// Configures service bus.
        /// </summary>
        /// <param name="busCfg">Service bus configuration.</param>
        /// <param name="host">Service bus host.</param>
        private void ConfigureServiceBus(IServiceBusBusFactoryConfigurator busCfg, IServiceBusHost host)
        {
            busCfg.UseJsonSerializer();
            busCfg.DefaultMessageTimeToLive = ConfigDefaults.MessageTimeToLive;
            busCfg.EnableDeadLetteringOnMessageExpiration = ConfigDefaults.EnableDeadLetteringOnMessageExpiration;
            busCfg.LockDuration = ConfigDefaults.LockDuration;
            busCfg.AutoDeleteOnIdle = ConfigDefaults.AutoDeleteOnIdle;
            busCfg.MaxDeliveryCount = ConfigDefaults.MaxDeliveryCount;
            busCfg.EnableDuplicateDetection(ConfigDefaults.DuplicateDetectionWindow);

            _busConfigurator?.Invoke(busCfg, host);
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

        #endregion
    }
}
