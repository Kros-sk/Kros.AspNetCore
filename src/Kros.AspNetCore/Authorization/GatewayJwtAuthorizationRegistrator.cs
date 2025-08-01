using Kros.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Registrator for Gateway JWT Authorization.
/// </summary>
public sealed class GatewayJwtAuthorizationRegistrator
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="services">Collection of app services.</param>
    internal GatewayJwtAuthorizationRegistrator(IServiceCollection services)
    {
        _services = Check.NotNull(services, nameof(services));
    }

    /// <summary>
    /// Configures gateway authorization to use hybrid cache.
    /// </summary>
    /// <remarks>
    /// Hybrid cache does not support sliding expiration. If only sliding expiration
    /// (<see cref="GatewayJwtAuthorizationOptions.CacheSlidingExpirationOffset"/>) is set
    /// it will be used as absolute expiration.
    /// </remarks>
    public GatewayJwtAuthorizationRegistrator WithHybridCache()
    {
        _services.Replace(ServiceDescriptor.Scoped<ICacheService, HybridCacheService>());
        return this;
    }
}
