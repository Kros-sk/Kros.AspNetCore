using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authentication
{
    /// <summary>
    /// Processes API key authentication using the request Basic auth header.
    /// </summary>
    public class ApiKeyBasicAuthenticationHandler : AuthenticationHandler<ApiKeyBasicAuthenticationOptions>
    {
        private readonly string _apiKeyHeaderName = HeaderNames.Authorization;
        private const string ApiKeyPrefix = "Basic ";
        private const string ApiKeyRole = "ApiKeyUser";

        /// <summary>
        /// Ctor.
        /// </summary>
        public ApiKeyBasicAuthenticationHandler(
            IOptionsMonitor<ApiKeyBasicAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        /// <inheritdoc/>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(_apiKeyHeaderName, out StringValues headerApiKeyvalues)
                || headerApiKeyvalues.Count == 0
                || string.IsNullOrEmpty(headerApiKeyvalues[0])
                || !headerApiKeyvalues[0].StartsWith(ApiKeyPrefix))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (headerApiKeyvalues[0] == $"{ApiKeyPrefix}{Options.ApiKey}")
            {
                List<Claim> claims = new()
                {
                    new Claim(ClaimTypes.Role, ApiKeyRole)
                };
                ClaimsIdentity identity = new(claims, Options.Scheme);
                AuthenticationTicket ticket = new(new ClaimsPrincipal(identity), Options.Scheme);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("Wrong API key"));
        }
    }
}
