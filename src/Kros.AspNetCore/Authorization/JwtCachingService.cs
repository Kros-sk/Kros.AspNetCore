using Kros.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Service for caching JWT tokens.
    /// </summary>
    internal class JwtCachingService
    {
        private readonly HybridCache _hybridCache;
        private readonly GatewayJwtAuthorizationOptions _jwtAuthorizationOptions;
        private static Regex _cacheRegex = null;

        /// <summary>
        /// Initializes a new instance of the JwtCachingService class.
        /// </summary>
        /// <param name="hybridCache">The hybrid cache instance.</param>
        /// <param name="jwtAuthorizationOptions">Authorization options.</param>
        public JwtCachingService(
            HybridCache hybridCache,
            GatewayJwtAuthorizationOptions jwtAuthorizationOptions)
        {
            _hybridCache = hybridCache ?? throw new ArgumentNullException(nameof(hybridCache));
            _jwtAuthorizationOptions = jwtAuthorizationOptions ?? throw new ArgumentNullException(nameof(jwtAuthorizationOptions));
            
            if (!string.IsNullOrWhiteSpace(_jwtAuthorizationOptions.CacheKeyUrlPathRegexPattern))
            {
                _cacheRegex = new Regex(_jwtAuthorizationOptions.CacheKeyUrlPathRegexPattern);
            }
        }

        /// <summary>
        /// Gets or creates a cached JWT token using the provided factory function.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="token">The authorization token.</param>
        /// <param name="jwtTokenFactory">Factory function to create JWT token if not cached.</param>
        /// <returns>The JWT token.</returns>
        public async Task<string> GetOrCreateJwtTokenAsync(
            HttpContext httpContext,
            StringValues token,
            Func<Task<string>> jwtTokenFactory)
        {
            if (!ShouldUseCache(httpContext.Request))
            {
                return await jwtTokenFactory();
            }

            string cacheKey = BuildCacheKey(httpContext, token);
            
            return await _hybridCache.GetOrCreateAsync(cacheKey, async (cancellationToken) =>
            {
                return await jwtTokenFactory();
            }, GetCacheEntryOptions(), cancellationToken: default);
        }

        /// <summary>
        /// Gets or creates a cached JWT token for hash-based authorization.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="hashValue">The hash value.</param>
        /// <param name="jwtTokenFactory">Factory function to create JWT token if not cached.</param>
        /// <returns>The JWT token.</returns>
        public async Task<string> GetOrCreateJwtTokenForHashAsync(
            HttpContext httpContext,
            StringValues hashValue,
            Func<Task<string>> jwtTokenFactory)
        {
            if (!ShouldUseCache(httpContext.Request))
            {
                return await jwtTokenFactory();
            }

            string cacheKey = GetKey(hashValue.ToString()).ToString();
            
            return await _hybridCache.GetOrCreateAsync(cacheKey, async (cancellationToken) =>
            {
                return await jwtTokenFactory();
            }, GetCacheEntryOptions(), cancellationToken: default);
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
            
            int key = GetKey(token, cacheKeyPart);
            return key.ToString();
        }

        /// <summary>
        /// Determines whether caching should be used for the given request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>True if caching should be used; otherwise, false.</returns>
        private bool ShouldUseCache(HttpRequest request)
        {
            return IsCacheAllowed() && !IsRequestPathAllowedForCache(request);
        }

        /// <summary>
        /// Gets the URL path for cache key based on regex pattern.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>The URL path for cache key or null if not found.</returns>
        private string GetUrlPathForCacheKey(HttpContext httpContext)
        {
            if (!string.IsNullOrWhiteSpace(_jwtAuthorizationOptions.CacheKeyUrlPathRegexPattern)
                && !string.IsNullOrWhiteSpace(httpContext.Request.Path))
            {
                Match match = _cacheRegex.Match(httpContext.Request.Path);
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
        private static int GetKey(StringValues value, string additionalKeyPart = null)
            => (additionalKeyPart is null) ? HashCode.Combine(value) : HashCode.Combine(value, additionalKeyPart);

        /// <summary>
        /// Gets the cache entry options based on configuration.
        /// </summary>
        /// <returns>The hybrid cache entry options.</returns>
        private HybridCacheEntryOptions GetCacheEntryOptions()
        {
            if (!IsCacheAllowed())
            {
                return new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.Zero // Don't cache if not allowed
                };
            }

            // HybridCache doesn't support sliding expiration directly
            // Use absolute expiration as the primary cache option
            if (_jwtAuthorizationOptions.CacheAbsoluteExpiration != TimeSpan.Zero)
            {
                return new HybridCacheEntryOptions
                {
                    Expiration = _jwtAuthorizationOptions.CacheAbsoluteExpiration
                };
            }
            
            // If only sliding expiration is configured, use it as absolute expiration
            if (_jwtAuthorizationOptions.CacheSlidingExpirationOffset != TimeSpan.Zero)
            {
                return new HybridCacheEntryOptions
                {
                    Expiration = _jwtAuthorizationOptions.CacheSlidingExpirationOffset
                };
            }

            return new HybridCacheEntryOptions();
        }

        /// <summary>
        /// Checks if the request path is allowed for caching.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>True if the path is in the ignored list; otherwise, false.</returns>
        private bool IsRequestPathAllowedForCache(HttpRequest request)
            => _jwtAuthorizationOptions.IgnoredPathForCache
            .Contains(request.Path.Value.TrimEnd('/'), StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if caching is allowed based on configuration.
        /// </summary>
        /// <returns>True if caching is allowed; otherwise, false.</returns>
        private bool IsCacheAllowed()
            => _jwtAuthorizationOptions.CacheSlidingExpirationOffset != TimeSpan.Zero
                || _jwtAuthorizationOptions.CacheAbsoluteExpiration != TimeSpan.Zero;
    }
}
