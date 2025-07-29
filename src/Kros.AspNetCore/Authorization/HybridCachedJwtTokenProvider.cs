using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Provider for JWT tokens with caching capabilities.
/// </summary>
internal class HybridCachedJwtTokenProvider : IJwtTokenProvider
{
    private readonly HybridCache _hybridCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GatewayJwtAuthorizationOptions _jwtAuthorizationOptions;
    private readonly IJwtTokenProvider _jwtProvider;
    private static Regex _cacheRegex = null;

    /// <summary>
    /// Initializes a new instance of the JwtCachingService class.
    /// </summary>
    /// <param name="hybridCache">The hybrid cache instance.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    /// <param name="jwtProvider">The JWT provider instance.</param>
    public HybridCachedJwtTokenProvider(
        HybridCache hybridCache,
        IHttpContextAccessor httpContextAccessor,
        IOptions<GatewayJwtAuthorizationOptions> jwtAuthorizationOptions,
        IJwtTokenProvider jwtProvider)
    {
        _hybridCache = Check.NotNull(hybridCache, nameof(hybridCache));
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
        if (!JwtCacheHelper.ShouldUseCache(_httpContextAccessor.HttpContext.Request, _jwtAuthorizationOptions))
        {
            return await _jwtProvider.GetJwtTokenAsync(token);
        }

        string cacheKey = JwtCacheHelper.BuildCacheKey(
            _httpContextAccessor.HttpContext,
            token,
            _jwtAuthorizationOptions,
            _cacheRegex);

        return await _hybridCache.GetOrCreateAsync(
            cacheKey,
            async (cancellationToken) =>await _jwtProvider.GetJwtTokenAsync(token),
            GetCacheEntryOptions(),
            cancellationToken: default);
    }

    /// <summary>
    /// Gets a JWT token for hash-based authorization.
    /// </summary>
    /// <param name="hashValue">The hash value.</param>
    /// <returns>The JWT token.</returns>
    public async Task<string> GetJwtTokenForHashAsync(StringValues hashValue)
    {
        if (!JwtCacheHelper.ShouldUseCache(_httpContextAccessor.HttpContext.Request, _jwtAuthorizationOptions))
        {
            return await _jwtProvider.GetJwtTokenForHashAsync(hashValue);
        }

        string cacheKey = JwtCacheHelper.BuildHashCacheKey(hashValue);

        return await _hybridCache.GetOrCreateAsync(
            cacheKey,
            async (cancellationToken) => await _jwtProvider.GetJwtTokenForHashAsync(hashValue),
            GetCacheEntryOptions(),
            cancellationToken: default);
    }

    /// <summary>
    /// Gets the cache entry options based on configuration.
    /// </summary>
    /// <returns>The hybrid cache entry options.</returns>
    private HybridCacheEntryOptions GetCacheEntryOptions()
    {
        if (_jwtAuthorizationOptions.CacheAbsoluteExpiration != TimeSpan.Zero)
        {
            return new()
            {
                Expiration = _jwtAuthorizationOptions.CacheAbsoluteExpiration
            };
        }

        // HybridCache doesn't support sliding expiration directly
        // If only sliding expiration is configured, use it as absolute expiration
        if (_jwtAuthorizationOptions.CacheSlidingExpirationOffset != TimeSpan.Zero)
        {
            return new()
            {
                Expiration = _jwtAuthorizationOptions.CacheSlidingExpirationOffset
            };
        }

        return new HybridCacheEntryOptions();
    }
}
