using Kros.Utils;
using System;

namespace Kros.AspNetCore.ServiceDiscovery
{
    /// <summary>
    /// Specifies service name providing data for given enum value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ServiceNameAttribute : Attribute
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        public ServiceNameAttribute(string serviceName)
        {
            ServiceName = Check.NotNull(serviceName, nameof(serviceName));
        }

        /// <summary>
        /// Service name.
        /// </summary>
        public string ServiceName { get; }
    }
}
