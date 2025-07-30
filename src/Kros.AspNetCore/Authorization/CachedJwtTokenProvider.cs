using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Cached JWT token provider that uses an abstracted cache service.
/// </summary>
internal class CachedJwtTokenProvider : IJwtTokenProvider
{
    private readonly ICacheService _cacheService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GatewayJwtAuthorizationOptions _jwtAuthorizationOptions;
    private readonly IJwtTokenProvider _jwtProvider;
    private readonly ICacheKeyBuilder _cacheKeyBuilder;

    /// <summary>
    /// Initializes a new instance of the CachedJwtTokenProvider class.
    /// </summary>
    /// <param name="cacheService">The cache service instance.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="jwtAuthorizationOptions">Authorization options.</param>
    /// <param name="jwtProvider">The JWT provider instance.</param>
    /// <param name="cacheKeyBuilder">The cache key builder instance.</param>
    public CachedJwtTokenProvider(
        ICacheService cacheService,
        IHttpContextAccessor httpContextAccessor,
        IOptions<GatewayJwtAuthorizationOptions> jwtAuthorizationOptions,
        IJwtTokenProvider jwtProvider,
        ICacheKeyBuilder cacheKeyBuilder)
    {
        _cacheService = Check.NotNull(cacheService, nameof(cacheService));
        _httpContextAccessor = Check.NotNull(httpContextAccessor, nameof(httpContextAccessor));
        _jwtAuthorizationOptions = Check.NotNull(jwtAuthorizationOptions.Value, nameof(jwtAuthorizationOptions));
        _jwtProvider = Check.NotNull(jwtProvider, nameof(jwtProvider));
        _cacheKeyBuilder = Check.NotNull(cacheKeyBuilder, nameof(cacheKeyBuilder));
    }

    /// <inheritdoc/>
    public async Task<string> GetJwtTokenAsync(StringValues token)
    {
        HttpRequest request = _httpContextAccessor.HttpContext.Request;
        
        if (!ShouldUseCache(request))
        {
            return await _jwtProvider.GetJwtTokenAsync(token);
        }

        string cacheKey = _cacheKeyBuilder.BuildCacheKey(_httpContextAccessor.HttpContext, token);

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            () => _jwtProvider.GetJwtTokenAsync(token));
    }

    /// <inheritdoc/>
    public async Task<string> GetJwtTokenForHashAsync(StringValues hashValue)
    {
        HttpRequest request = _httpContextAccessor.HttpContext.Request;
        
        if (!ShouldUseCache(request))
        {
            return await _jwtProvider.GetJwtTokenForHashAsync(hashValue);
        }

        string cacheKey = _cacheKeyBuilder.BuildHashCacheKey(hashValue);

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            () => _jwtProvider.GetJwtTokenForHashAsync(hashValue));
    }

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
}
