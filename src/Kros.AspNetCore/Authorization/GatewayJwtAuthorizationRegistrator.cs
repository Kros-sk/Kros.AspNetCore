using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

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
    /// Adds hybrid cache to the service collection and configures JWT token provider.
    /// </summary>
    /// <param name="setupAction">The setup action for the hybrid cache options.</param>
    /// <returns>The current instance of the <see cref="GatewayJwtAuthorizationRegistrator"/>.</returns>
    public GatewayJwtAuthorizationRegistrator WithHybridCache(
        Action<HybridCacheOptions> setupAction)
    {
        _services.AddHybridCache(setupAction);
        AddHybridCachedJwtTokenProvider();
        return this;
    }

    /// <summary>
    /// Adds hybrid cache to the service collection and configures JWT token provider.
    /// </summary>
    public GatewayJwtAuthorizationRegistrator WithHybridCache()
    {
        _services.AddHybridCache();
        AddHybridCachedJwtTokenProvider();
        return this;
    }

    private void AddHybridCachedJwtTokenProvider()
    {
        _services.AddScoped<IJwtTokenProvider>(services => new HybridCachedJwtTokenProvider(
            services.GetRequiredService<HybridCache>(),
            services.GetRequiredService<IHttpContextAccessor>(),
            services.GetRequiredService<IOptions<GatewayJwtAuthorizationOptions>>(),
            services.GetRequiredService<ApiJwtTokenProvider>()));
    }
}