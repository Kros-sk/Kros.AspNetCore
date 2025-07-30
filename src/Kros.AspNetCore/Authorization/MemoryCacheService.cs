using Kros.Utils;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Memory cache service implementation.
/// </summary>
internal class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the MemoryCacheService class.
    /// </summary>
    /// <param name="memoryCache">The memory cache instance.</param>
    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = Check.NotNull(memoryCache, nameof(memoryCache));
    }

    /// <inheritdoc/>
    public async Task<string> GetOrCreateAsync(
        string key,
        Func<Task<string>> factory,
        TimeSpan absoluteExpiration,
        TimeSpan slidingExpiration)
    {
        if (_memoryCache.TryGetValue(key, out string cachedValue))
        {
            return cachedValue;
        }

        string value = await factory();

        var cacheEntryOptions = new MemoryCacheEntryOptions();

        if (slidingExpiration != TimeSpan.Zero)
        {
            cacheEntryOptions.SetSlidingExpiration(slidingExpiration);
        }
        if (absoluteExpiration != TimeSpan.Zero)
        {
            cacheEntryOptions.SetAbsoluteExpiration(absoluteExpiration);
        }

        _memoryCache.Set(key, value, cacheEntryOptions);

        return value;
    }
}
