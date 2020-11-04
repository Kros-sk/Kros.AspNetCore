using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
            if (TryGetTokenValue(httpContext.Request.Headers, out string tokenValue)
                && _tokenHandler.CanReadToken(tokenValue))
            {
                JwtSecurityToken token = _tokenHandler.ReadJwtToken(tokenValue);
                httpContext.User.AddIdentity(new ClaimsIdentity(token.Claims));
            }

            await _next(httpContext);
        }

        private static bool TryGetTokenValue(IHeaderDictionary headers, out string tokenValue)
        {
            if (headers.TryGetValue(HeaderNames.Authorization, out StringValues authHeader))
            {
                string authHeaderToken = authHeader.First();
                if (authHeaderToken.StartsWith(AuthTokenPrefix))
                {
                    tokenValue = authHeaderToken.ToString().Substring(AuthTokenPrefix.Length);
                    return true;
                }
            }
            tokenValue = null;
            return false;
        }
    }
}
