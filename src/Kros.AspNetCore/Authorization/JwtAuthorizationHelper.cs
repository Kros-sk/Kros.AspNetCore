using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
        /// OAuth authentication scheme name.
        /// </summary>
        public const string OAuthSchemeName = "OAuthAuthorization";

        /// <summary>
        /// Create signed JWT token.
        /// </summary>
        /// <param name="userClaims">User claims.</param>
        /// <param name="secretKey">Key used for signing.</param>
        /// <param name="expires">Sets the value of the 'expiration' claim.</param>
        /// <returns>Jwt token.</returns>
        public static string CreateJwtTokenFromClaims(IEnumerable<Claim> userClaims, string secretKey, DateTime? expires = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(securityToken);
        }
    }
}
