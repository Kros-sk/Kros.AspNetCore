using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Default implementation of cache key builder for JWT token caching.
/// </summary>
internal class DefaultCacheKeyBuilder : ICacheKeyBuilder
{
    private const string CacheKeyPrefix = "jwtToken:v1:";

    private readonly GatewayJwtAuthorizationOptions _jwtAuthorizationOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static Regex _cacheRegex = null;

    /// <summary>
    /// Initializes a new instance of the DefaultCacheKeyBuilder class.
    /// </summary>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public DefaultCacheKeyBuilder(
        IOptions<GatewayJwtAuthorizationOptions> jwtAuthorizationOptions,
        IHttpContextAccessor httpContextAccessor)
    {
        _jwtAuthorizationOptions = Check.NotNull(jwtAuthorizationOptions.Value, nameof(jwtAuthorizationOptions));
        _httpContextAccessor = Check.NotNull(httpContextAccessor, nameof(httpContextAccessor));

        if (!string.IsNullOrWhiteSpace(_jwtAuthorizationOptions.CacheKeyUrlPathRegexPattern))
        {
            _cacheRegex = new Regex(_jwtAuthorizationOptions.CacheKeyUrlPathRegexPattern);
        }
    }

    /// <inheritdoc/>
    public string BuildCacheKey(StringValues token)
    {
        HttpContext httpContext = _httpContextAccessor.HttpContext;
        
        CacheHttpHeadersHelper.TryGetValue(
            httpContext.Request.Headers,
            _jwtAuthorizationOptions.CacheKeyHttpHeaders,
            out string cacheKeyPart);

        string urlPathForCache = GetUrlPathForCacheKey(httpContext);
        if (urlPathForCache != null)
        {
            cacheKeyPart += urlPathForCache;
        }

        return GetKey(token, cacheKeyPart);
    }

    /// <inheritdoc/>
    public string BuildHashCacheKey(StringValues hashValue) => GetKey(hashValue.ToString());

    /// <summary>
    /// Gets the URL path for cache key based on regex pattern.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The URL path for cache key or null if not found.</returns>
    private string GetUrlPathForCacheKey(HttpContext httpContext)
    {
        if (!string.IsNullOrWhiteSpace(_jwtAuthorizationOptions.CacheKeyUrlPathRegexPattern)
            && !string.IsNullOrWhiteSpace(httpContext.Request.Path)
            && _cacheRegex != null)
        {
            var match = _cacheRegex.Match(httpContext.Request.Path);
            if (match.Success)
            {
                return match.Groups.Values.Last().Value;
            }
        }
        return null;
    }

    /// <summary>
    /// Generates a SHA256 hash key from the given values.
    /// </summary>
    /// <param name="value">The primary value.</param>
    /// <param name="additionalKeyPart">Additional key part (optional).</param>
    /// <returns>The SHA256 hash as a Base64 URL-safe string.</returns>
    private static string GetKey(StringValues value, string additionalKeyPart = null)
    {
        string input = additionalKeyPart is null ? value.ToString() : $"{value}:{additionalKeyPart}";
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        string hash = Convert.ToBase64String(hashBytes);
        return CacheKeyPrefix + hash;
    }
}
