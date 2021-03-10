using Kros.AspNetCore.Options;
using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Middleware for identity server authorization with client credentials..
    /// </summary>
    internal class ClientCredentialsAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ClientCredentialsAuthorizationOptions _authorizationOptions;
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="next">Next middleware.</param>
        /// <param name="options">Client credentials authorization options.</param>
        public ClientCredentialsAuthorizationMiddleware(RequestDelegate next, ClientCredentialsAuthorizationOptions options)
        {
            _next = Check.NotNull(next, nameof(next));
            _authorizationOptions = Check.NotNull(options, nameof(options));
            Check.NotNullOrWhiteSpace(options.AuthorityUrl, nameof(options.AuthorityUrl));
            Check.NotNullOrWhiteSpace(options.Scope, nameof(options.AuthorityUrl));
            Check.NotNull(options.ClockSkew, nameof(options.ClockSkew));
        }

        /// <summary>
        /// HttpContext pipeline processing.
        /// </summary>
        /// <param name="httpContext">Http context.</param>
        public async Task Invoke(HttpContext httpContext)
        {
            await ValidateJwtToken(httpContext);
            await _next(httpContext);
        }

        private async Task ValidateJwtToken(HttpContext httpContext)
        {
            if (JwtAuthorizationHelper.TryGetTokenValue(httpContext.Request.Headers, out string value, true))
            {
                if (await IsAccessTokenValid(value))
                {
                    return;
                }
            }
            throw new UnauthorizedAccessException(Properties.Resources.AuthorizationServiceForbiddenRequest);
        }

        private async Task<bool> IsAccessTokenValid(string accessToken)
        {
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                _authorizationOptions.AuthorityUrl + "/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());

            JwtSecurityToken validatedToken = await ValidateToken(accessToken, configurationManager);

            return validatedToken != null;
        }

        private async Task<JwtSecurityToken> ValidateToken(
            string token,
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager,
            CancellationToken ct = default)
        {
            Check.NotNullOrWhiteSpace(token, nameof(token));

            OpenIdConnectConfiguration discoveryDocument = await configurationManager.GetConfigurationAsync(ct);
            var validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuer = _authorizationOptions.AuthorityUrl,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = discoveryDocument.SigningKeys,
                ValidateLifetime = true,
                ValidAudience = _authorizationOptions.Scope,
                ClockSkew = _authorizationOptions.ClockSkew
            };

            try
            {
                ClaimsPrincipal principal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, validationParameters, out SecurityToken rawValidatedToken);

                return (JwtSecurityToken)rawValidatedToken;
            }
            catch (SecurityTokenValidationException)
            {
                return null;
            }
        }
    }
}
