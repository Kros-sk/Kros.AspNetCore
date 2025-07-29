using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Provider for JWT tokens using memory caching.
/// </summary>
internal class MemoryCachedJwtTokenProvider : IJwtTokenProvider
{
    private readonly IMemoryCache _memoryCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GatewayJwtAuthorizationOptions _jwtAuthorizationOptions;
    private readonly IJwtTokenProvider _jwtProvider;
    private static Regex _cacheRegex = null;

    /// <summary>
    /// Initializes a new instance of the MemoryCachedJwtTokenProvider class.
    /// </summary>
    /// <param name="memoryCache">The memory cache instance.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    /// <param name="jwtProvider">The JWT provider instance.</param>
    public MemoryCachedJwtTokenProvider(
        IMemoryCache memoryCache,
        IHttpContextAccessor httpContextAccessor,
        IOptions<GatewayJwtAuthorizationOptions> jwtAuthorizationOptions,
        IJwtTokenProvider jwtProvider)
    {
        _memoryCache = Check.NotNull(memoryCache, nameof(memoryCache));
        _httpContextAccessor = Check.NotNull(httpContextAccessor, nameof(httpContextAccessor));
        _jwtAuthorizationOptions = Check.NotNull(jwtAuthorizationOptions.Value, nameof(jwtAuthorizationOptions));
        _jwtProvider = Check.NotNull(jwtProvider, nameof(jwtProvider));

        if (!string.IsNullOrWhiteSpace(_jwtAuthorizationOptions.CacheKeyUrlPathRegexPattern))
        {
            _cacheRegex = new Regex(_jwtAuthorizationOptions.CacheKeyUrlPathRegexPattern);
        }
    }

    /// <summary>
    /// Gets a JWT token for the given authorization token.
    /// </summary>
    /// <param name="token">The authorization token.</param>
    /// <returns>The JWT token.</returns>
    public async Task<string> GetJwtTokenAsync(StringValues token)
    {
        HttpRequest request = _httpContextAccessor.HttpContext.Request;
        
        if (!JwtCacheHelper.ShouldUseCache(request, _jwtAuthorizationOptions))
        {
            return await _jwtProvider.GetJwtTokenAsync(token);
        }

        string cacheKey = JwtCacheHelper.BuildCacheKey(
            _httpContextAccessor.HttpContext,
            token,
            _jwtAuthorizationOptions,
            _cacheRegex);

        if (_memoryCache.TryGetValue(cacheKey, out string cachedToken))
        {
            return cachedToken;
        }

        string jwtToken = await _jwtProvider.GetJwtTokenAsync(token);
        SetTokenToCache(cacheKey, jwtToken, request);

        return jwtToken;
    }

    /// <summary>
    /// Gets a JWT token for hash-based authorization.
    /// </summary>
    /// <param name="hashValue">The hash value.</param>
    /// <returns>The JWT token.</returns>
    public async Task<string> GetJwtTokenForHashAsync(StringValues hashValue)
    {
        HttpRequest request = _httpContextAccessor.HttpContext.Request;
        
        if (!JwtCacheHelper.ShouldUseCache(request, _jwtAuthorizationOptions))
        {
            return await _jwtProvider.GetJwtTokenForHashAsync(hashValue);
        }

        string cacheKey = JwtCacheHelper.BuildHashCacheKey(hashValue);

        if (_memoryCache.TryGetValue(cacheKey, out string cachedToken))
        {
            return cachedToken;
        }

        string jwtToken = await _jwtProvider.GetJwtTokenForHashAsync(hashValue);
        SetTokenToCache(cacheKey, jwtToken, request);

        return jwtToken;
    }

    /// <summary>
    /// Sets the JWT token to cache with appropriate expiration options.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="jwtToken">The JWT token to cache.</param>
    /// <param name="request">The HTTP request.</param>
    private void SetTokenToCache(string cacheKey, string jwtToken, HttpRequest request)
    {
        if (JwtCacheHelper.IsCacheAllowed(_jwtAuthorizationOptions) && 
            !JwtCacheHelper.IsRequestPathIgnoredForCache(request, _jwtAuthorizationOptions))
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions();

            if (_jwtAuthorizationOptions.CacheSlidingExpirationOffset != TimeSpan.Zero)
            {
                cacheEntryOptions.SetSlidingExpiration(_jwtAuthorizationOptions.CacheSlidingExpirationOffset);
            }
            if (_jwtAuthorizationOptions.CacheAbsoluteExpiration != TimeSpan.Zero)
            {
                cacheEntryOptions.SetAbsoluteExpiration(_jwtAuthorizationOptions.CacheAbsoluteExpiration);
            }

            _memoryCache.Set(cacheKey, jwtToken, cacheEntryOptions);
        }
    }
}
