using Kros.AspNetCore.Extensions;
using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Middleware for user authorization.
    /// </summary>
    internal class GatewayAuthorizationMiddleware
    {
        /// <summary>
        /// HttpClient name used for communication between ApiGateway and Authorization service.
        /// </summary>
        public const string AuthorizationHttpClientName = "JwtAuthorizationClientName";

        private readonly RequestDelegate _next;
        private readonly GatewayJwtAuthorizationOptions _jwtAuthorizationOptions;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="next">Next middleware.</param>
        /// <param name="jwtAuthorizationOptions">Authorization options.</param>
        public GatewayAuthorizationMiddleware(
            RequestDelegate next,
            GatewayJwtAuthorizationOptions jwtAuthorizationOptions)
        {
            _next = Check.NotNull(next, nameof(next));
            _jwtAuthorizationOptions = Check.NotNull(jwtAuthorizationOptions, nameof(jwtAuthorizationOptions));
        }

        /// <summary>
        /// HttpContext pipeline processing.
        /// </summary>
        /// <param name="httpContext">Http context.</param>
        /// <param name="httpClientFactory">Http client factory.</param>
        /// <param name="memoryCache">Cache for caching authorization token.</param>
        public async Task Invoke(
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache)
        {
            string userJwt = await GetUserAuthorizationJwtAsync(httpContext, httpClientFactory, memoryCache);

            if (!string.IsNullOrEmpty(userJwt))
            {
                AddUserProfileClaimsToIdentityAndHttpHeaders(httpContext, userJwt);
            }

            await _next(httpContext);
        }

        private async Task<string> GetUserAuthorizationJwtAsync(
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache)
        {
            if (httpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out StringValues value))
            {
                int key = GetKey(httpContext, value);

                if (!memoryCache.TryGetValue(key, out string jwtToken))
                {
                    var authUrl = $"{_jwtAuthorizationOptions.AuthorizationUrl}{httpContext.Request.Path.Value}";
                    jwtToken = await GetUserAuthorizationJwtAsync(httpContext, httpClientFactory, memoryCache, value, key, authUrl);
                }

                return jwtToken;
            }
            else if (httpContext.Request.Query.TryGetValue(_jwtAuthorizationOptions.HashParameterName, out StringValues hashValue))
            {
                int key = GetKey(httpContext, hashValue.ToString());
                if (!memoryCache.TryGetValue(key, out string jwtToken))
                {
                    var queryString = QueryString.Create(_jwtAuthorizationOptions.HashParameterName, hashValue.ToString());
                    var authUrl = $"{_jwtAuthorizationOptions.HashAuthorizationUrl}{queryString.ToUriComponent()}";
                    jwtToken = await GetUserAuthorizationJwtAsync(httpContext, httpClientFactory, memoryCache, StringValues.Empty, key, authUrl);
                }

                return jwtToken;
            }

            return string.Empty;
        }

        private async Task<string> GetUserAuthorizationJwtAsync(
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache,
            StringValues authHeader,
            int cacheKey,
            string authorizationUrl)
        {
            using (HttpClient client = httpClientFactory.CreateClient(AuthorizationHttpClientName))
            {
                if (authHeader.Any())
                {
                    client.DefaultRequestHeaders.Add(HeaderNames.Authorization, authHeader.ToString());
                }

                string jwtToken = await client.GetStringAndCheckResponseAsync(authorizationUrl,
                    new UnauthorizedAccessException(Properties.Resources.AuthorizationServiceForbiddenRequest));
                SetTokenToCache(memoryCache, cacheKey, jwtToken, httpContext.Request);

                return jwtToken;
            }
        }

        private void SetTokenToCache(IMemoryCache memoryCache, int key, string jwtToken, HttpRequest request)
        {
            if (_jwtAuthorizationOptions.CacheSlidingExpirationOffset != TimeSpan.Zero &&
                !_jwtAuthorizationOptions.IgnoredPathForCache.Contains(request.Path.Value.TrimEnd('/'), StringComparer.OrdinalIgnoreCase))
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(_jwtAuthorizationOptions.CacheSlidingExpirationOffset);

                memoryCache.Set(key, jwtToken, cacheEntryOptions);
            }
        }

        private static int GetKey(HttpContext httpContext, StringValues value)
            => HashCode.Combine(value, httpContext.Request.Path);

        private void AddUserProfileClaimsToIdentityAndHttpHeaders(HttpContext httpContext, string userJwtToken)
        {
            httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {userJwtToken}";
        }
    }
}
