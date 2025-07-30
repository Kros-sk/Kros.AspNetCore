using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Cached JWT token provider that uses an abstracted cache service.
/// </summary>
internal class CachedJwtTokenProvider : IJwtTokenProvider
{
    private const string CacheKeyPrefix = "JwtToken:";

    private readonly ICacheService _cacheService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GatewayJwtAuthorizationOptions _jwtAuthorizationOptions;
    private readonly IJwtTokenProvider _jwtProvider;
    private static Regex _cacheRegex = null;

    /// <summary>
    /// Initializes a new instance of the CachedJwtTokenProvider class.
    /// </summary>
    /// <param name="cacheService">The cache service instance.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    /// <param name="jwtProvider">The JWT provider instance.</param>
    public CachedJwtTokenProvider(
        ICacheService cacheService,
        IHttpContextAccessor httpContextAccessor,
        IOptions<GatewayJwtAuthorizationOptions> jwtAuthorizationOptions,
        IJwtTokenProvider jwtProvider)
    {
        _cacheService = Check.NotNull(cacheService, nameof(cacheService));
        _httpContextAccessor = Check.NotNull(httpContextAccessor, nameof(httpContextAccessor));
        _jwtAuthorizationOptions = Check.NotNull(jwtAuthorizationOptions.Value, nameof(jwtAuthorizationOptions));
        _jwtProvider = Check.NotNull(jwtProvider, nameof(jwtProvider));

        if (!string.IsNullOrWhiteSpace(_jwtAuthorizationOptions.CacheKeyUrlPathRegexPattern))
        {
            _cacheRegex = new Regex(_jwtAuthorizationOptions.CacheKeyUrlPathRegexPattern);
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetJwtTokenAsync(StringValues token)
    {
        HttpRequest request = _httpContextAccessor.HttpContext.Request;
        
        if (!ShouldUseCache(request))
        {
            return await _jwtProvider.GetJwtTokenAsync(token);
        }

        string cacheKey = BuildCacheKey(_httpContextAccessor.HttpContext, token);

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            () => _jwtProvider.GetJwtTokenAsync(token),
            _jwtAuthorizationOptions.CacheAbsoluteExpiration,
            _jwtAuthorizationOptions.CacheSlidingExpirationOffset);
    }

    /// <inheritdoc/>
    public async Task<string> GetJwtTokenForHashAsync(StringValues hashValue)
    {
        HttpRequest request = _httpContextAccessor.HttpContext.Request;
        
        if (!ShouldUseCache(request))
        {
            return await _jwtProvider.GetJwtTokenForHashAsync(hashValue);
        }

        string cacheKey = BuildHashCacheKey(hashValue);

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            () => _jwtProvider.GetJwtTokenForHashAsync(hashValue),
            _jwtAuthorizationOptions.CacheAbsoluteExpiration,
            _jwtAuthorizationOptions.CacheSlidingExpirationOffset);
    }

    /// <summary>
    /// Builds a cache key based on the HTTP context and token.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="token">The authorization token.</param>
    /// <returns>The cache key.</returns>
    private string BuildCacheKey(HttpContext httpContext, StringValues token)
    {
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

    /// <summary>
    /// Builds a cache key for hash-based authorization.
    /// </summary>
    /// <param name="hashValue">The hash value.</param>
    /// <returns>The cache key.</returns>
    private static string BuildHashCacheKey(StringValues hashValue) => GetKey(hashValue.ToString());

    /// <summary>
    /// Determines whether caching should be used for the given request.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>True if caching should be used; otherwise, false.</returns>
    private bool ShouldUseCache(HttpRequest request)
    {
        return IsCacheAllowed() && !IsRequestPathIgnoredForCache(request);
    }

    /// <summary>
    /// Checks if caching is allowed based on configuration.
    /// </summary>
    /// <returns>True if caching is allowed; otherwise, false.</returns>
    private bool IsCacheAllowed()
        => _jwtAuthorizationOptions.CacheSlidingExpirationOffset != TimeSpan.Zero
            || _jwtAuthorizationOptions.CacheAbsoluteExpiration != TimeSpan.Zero;

    /// <summary>
    /// Checks if the request path is ignored for caching.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>True if the path is in the ignored list; otherwise, false.</returns>
    private bool IsRequestPathIgnoredForCache(HttpRequest request)
        => _jwtAuthorizationOptions.IgnoredPathForCache
        .Contains(request.Path.Value.TrimEnd('/'), StringComparer.OrdinalIgnoreCase);

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
    /// Generates a hash key from the given values.
    /// </summary>
    /// <param name="value">The primary value.</param>
    /// <param name="additionalKeyPart">Additional key part (optional).</param>
    /// <returns>The hash code.</returns>
    private static string GetKey(StringValues value, string additionalKeyPart = null)
    {
        int key = (additionalKeyPart is null) ? HashCode.Combine(value) : HashCode.Combine(value, additionalKeyPart);
        return CacheKeyPrefix + key.ToString();
    }
}
