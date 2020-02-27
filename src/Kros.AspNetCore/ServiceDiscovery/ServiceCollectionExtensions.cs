using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Kros.AspNetCore.ServiceDiscovery
{
    /// <summary>
    /// Extensions for <see cref="IConfiguration"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the service discovery.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="options">The configure.</param>
        public static IServiceCollection AddServiceDiscovery(
            this IServiceCollection services,
            Action<ServiceDiscoveryOption> options = null)
        {
            options?.Invoke(ServiceDiscoveryOption.Default);

            services.AddScoped<IServiceDiscoveryProvider>((f) =>
                new ServiceDiscoveryProvider(f.GetService<IConfiguration>(), ServiceDiscoveryOption.Default));

            return services;
        }
    }
}
