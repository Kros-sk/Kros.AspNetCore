using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;

namespace Kros.MassTransit.AzureServiceBus
{
    /// <summary>
    /// Options for Azure service bus.
    /// </summary>
    public class AzureServiceBusOptions
    {
        /// <summary>
        /// Connection string for service bus.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Base service bus endpoint.
        /// </summary>
        public string Endpoint => ServiceBusConnectionStringProperties.Parse(ConnectionString).Endpoint.ToString();

        /// <summary>
        /// Token time to live in seconds.
        /// </summary>
        [Obsolete("This property is not used anymore.")]
        public int TokenTimeToLive { get; set; }

        /// <summary>
        /// Topic name prefix.
        /// </summary>
        /// <remarks>Used by MassTransit for creating topic names.</remarks>
        public string TopicNamePrefix { get; set; }

        /// <summary>
        /// Endpoint name prefix.
        /// </summary>
        /// <remarks>Used by MassTransit for creating endpoint names(queues, subscriptions).</remarks>
        public string EndpointNamePrefix { get; set; }

        /// <summary>
        /// Dictionary of supported service bus endpoints.
        /// </summary>
        public Dictionary<string, AzureServiceBusEndpoint> Endpoints { get; set; }

        /// <summary>
        /// The interval retry options.
        /// </summary>
        public AzureServiceBusIntervalRetryOptions IntervalRetry { get; set; }

        /// <summary>
        /// Information about service bus endpoint.
        /// </summary>
        public class AzureServiceBusEndpoint
        {
            /// <summary>
            /// Endpoint name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Endpoint type.
            /// </summary>
            /// <remarks>Only for information purposes, can be empty.</remarks>
            public string Type { get; set; }
        }

        /// <summary>
        /// Service bus interval retry options.
        /// </summary>
        public class AzureServiceBusIntervalRetryOptions
        {
            /// <summary>
            /// The retry attempts limit.
            /// </summary>
            public int Limit { get; set; }

            /// <summary>
            /// The retry interval in milliseconds.
            /// </summary>
            public int Interval { get; set; }
        }
    }
}
