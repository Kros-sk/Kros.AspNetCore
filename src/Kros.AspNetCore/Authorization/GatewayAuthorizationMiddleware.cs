using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
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
        public async Task Invoke(
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory)
        {
            var userJwt = await GetUserAuthorizationJwtAsync(httpContext, httpClientFactory);

            if (!string.IsNullOrEmpty(userJwt))
            {
                AddUserProfileClaimsToIdentityAndHttpHeaders(httpContext, userJwt);
            }

            await _next(httpContext);
        }

        private async Task<string> GetUserAuthorizationJwtAsync(HttpContext httpContext, IHttpClientFactory httpClientFactory)
        {
            if (httpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out StringValues value))
            {
                using (var client = httpClientFactory.CreateClient(AuthorizationHttpClientName))
                {
                    client.DefaultRequestHeaders.Add(HeaderNames.Authorization, value.ToString());

                    var response = await client.GetAsync(_jwtAuthorizationOptions.AuthorizationUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
            }

            return string.Empty;
        }

        private void AddUserProfileClaimsToIdentityAndHttpHeaders(HttpContext httpContext, string userJwtToken)
        {
            httpContext.Request.Headers[HeaderNames.Authorization] = $"Bearer {userJwtToken}";
        }
    }
}