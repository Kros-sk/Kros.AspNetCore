using Kros.AspNetCore.Extensions;
using Kros.AspNetCore.ServiceDiscovery;
using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
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
        /// <param name="jwtCachingService">JWT caching service.</param>
        /// <param name="serviceDiscoveryProvider">The service discovery provider.</param>
        public async Task Invoke(
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory,
            IJwtCachingService jwtCachingService,
            IServiceDiscoveryProvider serviceDiscoveryProvider)
        {
            string userJwt = await GetUserAuthorizationJwtAsync(
                httpContext,
                httpClientFactory,
                jwtCachingService,
                serviceDiscoveryProvider);

            if (!string.IsNullOrEmpty(userJwt))
            {
                AddUserProfileClaimsToIdentityAndHttpHeaders(httpContext, userJwt);
            }

            await _next(httpContext);
        }

        private async Task<string> GetUserAuthorizationJwtAsync(
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory,
            IJwtCachingService jwtCachingService,
            IServiceDiscoveryProvider serviceDiscoveryProvider)
        {
            if (JwtAuthorizationHelper.TryGetTokenValue(httpContext.Request.Headers, out string token))
            {
                return await jwtCachingService.GetOrCreateJwtTokenAsync(
                    httpContext,
                    token,
                    async () =>
                    {
                        string authUrl = _jwtAuthorizationOptions.GetAuthorizationUrl(serviceDiscoveryProvider) + httpContext.Request.Path.Value;
                        return await GetUserAuthorizationJwtAsync(
                            httpContext,
                            httpClientFactory,
                            token,
                            authUrl);
                    });
            }
            else if (!string.IsNullOrEmpty(_jwtAuthorizationOptions.HashParameterName)
                && httpContext.Request.Query.TryGetValue(_jwtAuthorizationOptions.HashParameterName, out StringValues hashValue))
            {
                return await jwtCachingService.GetOrCreateJwtTokenForHashAsync(
                    httpContext,
                    hashValue,
                    async () =>
                    {
                        UriBuilder uriBuilder = new(_jwtAuthorizationOptions.GetHashAuthorization(serviceDiscoveryProvider));
                        uriBuilder.Query = QueryString.Create(
                            _jwtAuthorizationOptions.HashParameterName,
                            hashValue.ToString()).ToUriComponent();
                        return await GetUserAuthorizationJwtAsync(
                            httpContext,
                            httpClientFactory,
                            StringValues.Empty,
                            uriBuilder.Uri.ToString());
                    });
            }

            return string.Empty;
        }

        private async Task<string> GetUserAuthorizationJwtAsync(
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory,
            StringValues authHeader,
            string authorizationUrl)
        {
            using (HttpClient client = httpClientFactory.CreateClient(AuthorizationHttpClientName))
            {
                if (authHeader.Any())
                {
                    client.DefaultRequestHeaders.Add(HeaderNames.Authorization, authHeader.ToString());
                }
                if (_jwtAuthorizationOptions.ForwardedHeaders.Any())
                {
                    AddForwardedHeaders(client, httpContext.Request.Headers);
                }

                string jwtToken = await client.GetStringAndCheckResponseAsync(authorizationUrl,
                    new UnauthorizedAccessException(Properties.Resources.AuthorizationServiceForbiddenRequest));

                return jwtToken;
            }
        }

        private void AddForwardedHeaders(HttpClient client, IHeaderDictionary headers)
        {
            foreach (string headerName in _jwtAuthorizationOptions.ForwardedHeaders)
            {
                if (headers.TryGetValue(headerName, out StringValues value))
                {
                    client.DefaultRequestHeaders.Add(headerName, (IEnumerable<string>)value);
                }
            }
        }

        private static void AddUserProfileClaimsToIdentityAndHttpHeaders(HttpContext httpContext, string userJwtToken)
            => httpContext.Request.Headers[HeaderNames.Authorization] = $"{JwtAuthorizationHelper.AuthTokenPrefix} {userJwtToken}";
    }
}
