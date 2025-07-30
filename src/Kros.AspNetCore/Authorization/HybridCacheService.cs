using Kros.Utils;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Hybrid cache service implementation.
/// </summary>
internal class HybridCacheService : ICacheService
{
    private readonly HybridCache _hybridCache;
    private readonly GatewayJwtAuthorizationOptions _jwtAuthorizationOptions;

    /// <summary>
    /// Initializes a new instance of the HybridCacheService class.
    /// </summary>
    /// <param name="hybridCache">The hybrid cache instance.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    public HybridCacheService(HybridCache hybridCache, IOptions<GatewayJwtAuthorizationOptions> jwtAuthorizationOptions)
    {
        _hybridCache = Check.NotNull(hybridCache, nameof(hybridCache));
        _jwtAuthorizationOptions = Check.NotNull(jwtAuthorizationOptions.Value, nameof(jwtAuthorizationOptions));
    }

    /// <inheritdoc/>
    public async Task<string> GetOrCreateAsync(
        string key,
        Func<Task<string>> factory)
    {
        HybridCacheEntryOptions cacheEntryOptions = GetCacheEntryOptions();

        return await _hybridCache.GetOrCreateAsync(
            key,
            async (cancellationToken) => await factory(),
            cacheEntryOptions,
            cancellationToken: default);
    }

    /// <summary>
    /// Gets the cache entry options based on configuration.
    /// </summary>
    /// <returns>The hybrid cache entry options.</returns>
    private HybridCacheEntryOptions GetCacheEntryOptions()
    {
        if (_jwtAuthorizationOptions.CacheAbsoluteExpiration != TimeSpan.Zero)
        {
            return new HybridCacheEntryOptions
            {
                Expiration = _jwtAuthorizationOptions.CacheAbsoluteExpiration
            };
        }

        // HybridCache doesn't support sliding expiration directly
        // If only sliding expiration is configured, use it as absolute expiration
        if (_jwtAuthorizationOptions.CacheSlidingExpirationOffset != TimeSpan.Zero)
        {
            return new HybridCacheEntryOptions
            {
                Expiration = _jwtAuthorizationOptions.CacheSlidingExpirationOffset
            };
        }

        return new HybridCacheEntryOptions();
    }
}
