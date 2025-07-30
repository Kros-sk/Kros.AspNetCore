using Kros.Utils;
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Hybrid cache service implementation.
/// </summary>
internal class HybridCacheService : ICacheService
{
    private readonly HybridCache _hybridCache;

    /// <summary>
    /// Initializes a new instance of the HybridCacheService class.
    /// </summary>
    /// <param name="hybridCache">The hybrid cache instance.</param>
    public HybridCacheService(HybridCache hybridCache)
    {
        _hybridCache = Check.NotNull(hybridCache, nameof(hybridCache));
    }

    /// <inheritdoc/>
    public async Task<string> GetOrCreateAsync(
        string key,
        Func<Task<string>> factory,
        TimeSpan absoluteExpiration,
        TimeSpan slidingExpiration)
    {
        HybridCacheEntryOptions cacheEntryOptions = GetCacheEntryOptions(absoluteExpiration, slidingExpiration);

        return await _hybridCache.GetOrCreateAsync(
            key,
            async (cancellationToken) => await factory(),
            cacheEntryOptions,
            cancellationToken: default);
    }

    /// <summary>
    /// Gets the cache entry options based on expiration settings.
    /// </summary>
    /// <param name="absoluteExpiration">Absolute expiration time.</param>
    /// <param name="slidingExpiration">Sliding expiration time.</param>
    /// <returns>The hybrid cache entry options.</returns>
    private static HybridCacheEntryOptions GetCacheEntryOptions(TimeSpan absoluteExpiration, TimeSpan slidingExpiration)
    {
        if (absoluteExpiration != TimeSpan.Zero)
        {
            return new HybridCacheEntryOptions
            {
                Expiration = absoluteExpiration
            };
        }

        // HybridCache doesn't support sliding expiration directly
        // If only sliding expiration is configured, use it as absolute expiration
        if (slidingExpiration != TimeSpan.Zero)
        {
            return new HybridCacheEntryOptions
            {
                Expiration = slidingExpiration
            };
        }

        return new HybridCacheEntryOptions();
    }
}
