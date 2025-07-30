using System;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Interface for cache service operations.
/// </summary>
internal interface ICacheService
{
    /// <summary>
    /// Gets or creates a cached value.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory function to create the value if not found in cache.</param>
    /// <param name="absoluteExpiration">Absolute expiration time.</param>
    /// <param name="slidingExpiration">Sliding expiration time.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<string> GetOrCreateAsync(
        string key,
        Func<Task<string>> factory,
        TimeSpan absoluteExpiration,
        TimeSpan slidingExpiration);
}
