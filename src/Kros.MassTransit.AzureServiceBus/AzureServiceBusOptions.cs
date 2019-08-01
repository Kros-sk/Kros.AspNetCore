using Microsoft.Azure.ServiceBus;
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
        public string Endpoint
        {
            get
            {
                string endpoint = new ServiceBusConnectionStringBuilder(ConnectionString).Endpoint;
                if (!string.IsNullOrEmpty(endpoint) && endpoint.EndsWith("/"))
                {
                    return endpoint;
                }
                else
                {
                    return endpoint + "/";
                }
            }
        }

        /// <summary>
        /// Token time to live in seconds. If 0, default value <see cref=" ConfigDefaults.TokenTimeToLive"/> is used.
        /// </summary>
        public int TokenTimeToLive { get; set; }

        /// <summary>
        /// Dictionary of supported service bus endpoints.
        /// </summary>
        public Dictionary<string, AzureServiceBusEndpoint> Endpoints { get; set; }

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
    }
}
