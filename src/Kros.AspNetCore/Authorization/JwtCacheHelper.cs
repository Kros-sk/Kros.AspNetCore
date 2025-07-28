using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Helper class for JWT caching operations including cache key building, validation, and configuration checks.
/// </summary>
internal static class JwtCacheHelper
{
    /// <summary>
    /// Builds a cache key based on the HTTP context and token.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="token">The authorization token.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    /// <param name="cacheRegex">Compiled regex for URL path pattern.</param>
    /// <returns>The cache key.</returns>
    public static string BuildCacheKey(
        HttpContext httpContext,
        StringValues token,
        GatewayJwtAuthorizationOptions jwtAuthorizationOptions,
        Regex cacheRegex)
    {
        CacheHttpHeadersHelper.TryGetValue(
            httpContext.Request.Headers,
            jwtAuthorizationOptions.CacheKeyHttpHeaders,
            out string cacheKeyPart);

        string urlPathForCache = GetUrlPathForCacheKey(httpContext, jwtAuthorizationOptions, cacheRegex);
        if (urlPathForCache != null)
        {
            cacheKeyPart += urlPathForCache;
        }

        int key = GetKey(token, cacheKeyPart);
        return key.ToString();
    }

    /// <summary>
    /// Builds a cache key for hash-based authorization.
    /// </summary>
    /// <param name="hashValue">The hash value.</param>
    /// <returns>The cache key.</returns>
    public static string BuildHashCacheKey(StringValues hashValue)
    {
        int key = GetKey(hashValue.ToString());
        return key.ToString();
    }

    /// <summary>
    /// Gets the URL path for cache key based on regex pattern.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    /// <param name="cacheRegex">Compiled regex for URL path pattern.</param>
    /// <returns>The URL path for cache key or null if not found.</returns>
    public static string GetUrlPathForCacheKey(
        HttpContext httpContext,
        GatewayJwtAuthorizationOptions jwtAuthorizationOptions,
        Regex cacheRegex)
    {
        if (!string.IsNullOrWhiteSpace(jwtAuthorizationOptions.CacheKeyUrlPathRegexPattern)
            && !string.IsNullOrWhiteSpace(httpContext.Request.Path)
            && cacheRegex != null)
        {
            var match = cacheRegex.Match(httpContext.Request.Path);
            if (match.Success)
            {
                return match.Groups.Values.Last().Value;
            }
        }
        return null;
    }

    /// <summary>
    /// Determines whether caching should be used for the given request.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    /// <returns>True if caching should be used; otherwise, false.</returns>
    public static bool ShouldUseCache(HttpRequest request, GatewayJwtAuthorizationOptions jwtAuthorizationOptions)
    {
        return IsCacheAllowed(jwtAuthorizationOptions) && !IsRequestPathIgnoredForCache(request, jwtAuthorizationOptions);
    }

    /// <summary>
    /// Checks if caching is allowed based on configuration.
    /// </summary>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    /// <returns>True if caching is allowed; otherwise, false.</returns>
    public static bool IsCacheAllowed(GatewayJwtAuthorizationOptions jwtAuthorizationOptions)
        => jwtAuthorizationOptions.CacheSlidingExpirationOffset != TimeSpan.Zero
            || jwtAuthorizationOptions.CacheAbsoluteExpiration != TimeSpan.Zero;

    /// <summary>
    /// Checks if the request path is ignored for caching.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    /// <returns>True if the path is in the ignored list; otherwise, false.</returns>
    public static bool IsRequestPathIgnoredForCache(HttpRequest request, GatewayJwtAuthorizationOptions jwtAuthorizationOptions)
        => jwtAuthorizationOptions.IgnoredPathForCache
        .Contains(request.Path.Value.TrimEnd('/'), StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Generates a hash key from the given values.
    /// </summary>
    /// <param name="value">The primary value.</param>
    /// <param name="additionalKeyPart">Additional key part (optional).</param>
    /// <returns>The hash code.</returns>
    public static int GetKey(StringValues value, string additionalKeyPart = null)
        => (additionalKeyPart is null) ? HashCode.Combine(value) : HashCode.Combine(value, additionalKeyPart);
}
