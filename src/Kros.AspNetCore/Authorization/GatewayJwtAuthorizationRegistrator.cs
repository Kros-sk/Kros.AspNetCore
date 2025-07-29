using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
    /// Configures JWT token provider to use hybrid cache.
    /// </summary>
    public GatewayJwtAuthorizationRegistrator WithHybridCache()
    {
        _services.AddScoped<IJwtTokenProvider>(services => new HybridCachedJwtTokenProvider(
            services.GetRequiredService<HybridCache>(),
            services.GetRequiredService<IHttpContextAccessor>(),
            services.GetRequiredService<IOptions<GatewayJwtAuthorizationOptions>>(),
            services.GetRequiredService<ApiJwtTokenProvider>()));
        return this;
    }

    /// <summary>
    /// Configures JWT token provider to use memory cache.
    /// </summary>
    /// <returns>Gateway JWT authorization registrator.</returns>
    public GatewayJwtAuthorizationRegistrator WithMemoryCache()
    {
        _services.AddScoped<IJwtTokenProvider>(services => new MemoryCachedJwtTokenProvider(
            services.GetRequiredService<IMemoryCache>(),
            services.GetRequiredService<IHttpContextAccessor>(),
            services.GetRequiredService<IOptions<GatewayJwtAuthorizationOptions>>(),
            services.GetRequiredService<ApiJwtTokenProvider>()));
        return this;
    }
}
