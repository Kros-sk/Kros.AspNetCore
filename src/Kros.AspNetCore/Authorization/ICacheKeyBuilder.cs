using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Interface for building cache keys for JWT token caching.
/// </summary>
public interface ICacheKeyBuilder
{
    /// <summary>
    /// Builds a cache key.
    /// </summary>
    /// <param name="token">The authorization token.</param>
    /// <returns>The cache key.</returns>
    string BuildCacheKey(StringValues token);

    /// <summary>
    /// Builds a cache key for hash-based authorization.
    /// </summary>
    /// <param name="hashValue">The hash value.</param>
    /// <returns>The cache key.</returns>
    string BuildHashCacheKey(StringValues hashValue);
}
