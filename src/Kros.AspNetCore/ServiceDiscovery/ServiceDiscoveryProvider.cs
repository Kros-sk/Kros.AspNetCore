using Kros.AspNetCore.Properties;
using Kros.Extensions;
using Kros.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Kros.AspNetCore.ServiceDiscovery
{
    /// <summary>
    /// Provider for obtaining service address from configuration.
    /// </summary>
    /// <seealso cref="Kros.AspNetCore.ServiceDiscovery.IServiceDiscoveryProvider" />
    public class ServiceDiscoveryProvider : IServiceDiscoveryProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ServiceDiscoveryOptions _option;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDiscoveryProvider"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="option">The option.</param>
        public ServiceDiscoveryProvider(IConfiguration configuration, ServiceDiscoveryOptions option)
        {
            _configuration = Check.NotNull(configuration, nameof(configuration));
            _option = Check.NotNull(option, nameof(option));
        }

        /// <inheritdoc />
        public Uri GetPath(string serviceName, string pathName)
        {
            var uriBuilder = new UriBuilder(GetService(serviceName));

            string path = _configuration.GetValue<string>($"{_option.SectionName}:{serviceName}:Paths:{pathName}");

            if (path.IsNullOrEmpty())
            {
                throw new ArgumentException(string.Format(Resources.PathDefinitionDoesntExist, serviceName, pathName));
            }

            uriBuilder.Path = path;

            return uriBuilder.Uri;
        }

        /// <inheritdoc />
        public Uri GetPath(Enum serviceType, string pathName)
            => GetPath(GetServiceNameFromEnumAttribute(serviceType), pathName);

        /// <inheritdoc />
        public Uri GetService(string serviceName)
        {
            string uri = _configuration.GetValue<string>($"{_option.SectionName}:{serviceName}:DownstreamPath");

            if (!uri.IsNullOrEmpty())
            {
                return new Uri(uri);
            }
            else if (_option.AllowServiceNameAsHost)
            {
                return new UriBuilder(_option.Scheme, serviceName, _option.Port).Uri;
            }
            else
            {
                throw new ArgumentException(
                    string.Format(Resources.ServiceDefinitionDoesntExist, serviceName, _option.SectionName));
            }
        }

        /// <inheritdoc />
        public Uri GetService(Enum serviceType)
            => GetService(GetServiceNameFromEnumAttribute(serviceType));

        private string GetServiceNameFromEnumAttribute(Enum enumValue)
        {
            var serviceNameAttribute = (ServiceNameAttribute)enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault()
                .GetCustomAttributes(typeof(ServiceNameAttribute), false).FirstOrDefault();
            if (serviceNameAttribute == null)
            {
                throw new ArgumentException(
                    string.Format(
                        Resources.ServiceTypeWithtouAttribute,
                        enumValue.ToString(),
                        enumValue.GetType().Name,
                        nameof(ServiceNameAttribute)));
            }
            return serviceNameAttribute.ServiceName;
        }
    }
}
