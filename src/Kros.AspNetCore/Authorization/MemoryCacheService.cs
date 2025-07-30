using Kros.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Memory cache service implementation.
/// </summary>
internal class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly GatewayJwtAuthorizationOptions _jwtAuthorizationOptions;

    /// <summary>
    /// Initializes a new instance of the MemoryCacheService class.
    /// </summary>
    /// <param name="memoryCache">The memory cache instance.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    public MemoryCacheService(IMemoryCache memoryCache, IOptions<GatewayJwtAuthorizationOptions> jwtAuthorizationOptions)
    {
        _memoryCache = Check.NotNull(memoryCache, nameof(memoryCache));
        _jwtAuthorizationOptions = Check.NotNull(jwtAuthorizationOptions.Value, nameof(jwtAuthorizationOptions));
    }

    /// <inheritdoc/>
    public async Task<string> GetOrCreateAsync(
        string key,
        Func<Task<string>> factory)
    {
        if (_memoryCache.TryGetValue(key, out string cachedValue))
        {
            return cachedValue;
        }

        string value = await factory();

        var cacheEntryOptions = new MemoryCacheEntryOptions();

        if (_jwtAuthorizationOptions.CacheSlidingExpirationOffset != TimeSpan.Zero)
        {
            cacheEntryOptions.SetSlidingExpiration(_jwtAuthorizationOptions.CacheSlidingExpirationOffset);
        }
        if (_jwtAuthorizationOptions.CacheAbsoluteExpiration != TimeSpan.Zero)
        {
            cacheEntryOptions.SetAbsoluteExpiration(_jwtAuthorizationOptions.CacheAbsoluteExpiration);
        }

        _memoryCache.Set(key, value, cacheEntryOptions);

        return value;
    }
}
