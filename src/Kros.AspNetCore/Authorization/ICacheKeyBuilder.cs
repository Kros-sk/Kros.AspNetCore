using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Interface for building cache keys for JWT token caching.
/// </summary>
public interface ICacheKeyBuilder
{
    /// <summary>
    /// Builds a cache key based on the HTTP context and token.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="token">The authorization token.</param>
    /// <returns>The cache key.</returns>
    string BuildCacheKey(HttpContext httpContext, StringValues token);

    /// <summary>
    /// Builds a cache key for hash-based authorization.
    /// </summary>
    /// <param name="hashValue">The hash value.</param>
    /// <returns>The cache key.</returns>
    string BuildHashCacheKey(StringValues hashValue);
}
