using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Helper class for working with JWT token.
    /// </summary>
    public static class JwtAuthorizationHelper
    {
        /// <summary>
        /// Jwt authentication scheme name.
        /// </summary>
        public const string JwtSchemeName = "JwtAuthorization";

        /// <summary>
        /// Hash jwt authentication scheme name.
        /// </summary>
        public const string HashJwtSchemeName = "JwtHashAuthorization";

        /// <summary>
        /// OAuth authentication scheme name.
        /// </summary>
        public const string OAuthSchemeName = "OAuthAuthorization";

        /// <summary>
        /// Authorization token prefix.
        /// </summary>
        public const string AuthTokenPrefix = "Bearer";

        /// <summary>
        /// Create signed JWT token.
        /// </summary>
        /// <param name="userClaims">User claims.</param>
        /// <param name="secretKey">Key used for signing.</param>
        /// <param name="expires">Sets the value of the 'expiration' claim.</param>
        /// <returns>Jwt token.</returns>
        public static string CreateJwtTokenFromClaims(IEnumerable<Claim> userClaims, string secretKey, DateTime? expires = null)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(secretKey);
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(securityToken);
        }

        /// <summary>
        /// Gets the token from header associated with the specified key <see cref="HeaderNames.Authorization"/>.
        /// </summary>
        /// <param name="headers">Http headers.</param>
        /// <param name="tokenValue">When this method returns, the value associated with the token,
        /// if the key is found; otherwise null.</param>
        /// <param name="removePrefix">True if token prefix (Bearer) should be removed, oherwise false.</param>
        /// <returns>True if the header contains an element with the specified key<see cref="HeaderNames.Authorization"/>.
        /// Otherwise false.</returns>
        public static bool TryGetTokenValue(IHeaderDictionary headers, out string tokenValue, bool removePrefix = false)
        {
            if (headers.TryGetValue(HeaderNames.Authorization, out StringValues authHeader))
            {
                tokenValue = authHeader.First();
                if (removePrefix && tokenValue.StartsWith(AuthTokenPrefix))
                {
                    tokenValue = tokenValue[(AuthTokenPrefix.Length + 1)..];
                }
                return true;
            }
            tokenValue = null;
            return false;
        }
    }
}
