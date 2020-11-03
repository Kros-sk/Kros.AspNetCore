using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Adds claims from jwt bearer token to current request.
    /// </summary>
    public class JwtBearerClaimsMiddleware
    {
        private readonly RequestDelegate _next;

        private const string AuthTokenPrefix = "Bearer ";
        private readonly JwtSecurityTokenHandler _tokenHandler;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="next">The next middleware.</param>
        /// <param name="tokenHandler">The token handler.</param>
        public JwtBearerClaimsMiddleware(RequestDelegate next, JwtSecurityTokenHandler tokenHandler)
        {
            _next = Check.NotNull(next, nameof(next));
            _tokenHandler = Check.NotNull(tokenHandler, nameof(tokenHandler));
        }

        /// <summary>
        /// HttpContext pipeline processing.
        /// </summary>
        /// <param name="httpContext">The http context.</param>
        public async Task Invoke(HttpContext httpContext)
        {
            string authHeader = httpContext.Request.Headers[HeaderNames.Authorization];
            if (authHeader.StartsWith(AuthTokenPrefix))
            {
                string tokenValue = authHeader.Substring(AuthTokenPrefix.Length);
                JwtSecurityToken token = _tokenHandler.ReadJwtToken(tokenValue);
                httpContext.User.AddIdentity(new ClaimsIdentity(token.Claims));
            }

            await _next(httpContext);
        }
    }
}
