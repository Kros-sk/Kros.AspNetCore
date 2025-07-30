using Kros.Utils;
using Microsoft.Extensions.DependencyInjection;

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
    public GatewayJwtAuthorizationRegistrator WithHybridCache()
    {
        _services.AddScoped<ICacheService, HybridCacheService>();
        return this;
    }

    /// <summary>
    /// Configures gateway authorization to use memory cache.
    /// </summary>
    /// <returns>Gateway JWT authorization registrator.</returns>
    public GatewayJwtAuthorizationRegistrator WithMemoryCache()
    {
        _services.AddScoped<ICacheService, MemoryCacheService>();
        return this;
    }
}
