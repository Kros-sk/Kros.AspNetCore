using System.Linq;
using System.Security.Claims;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Claims principal extensions for <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Return user id from user claims.
        /// </summary>
        /// <param name="userClaims">All user claims.</param>
        /// <returns>User id.</returns>
        public static int GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            if (int.TryParse(GetValueFromUserClaims(claimsPrincipal, UserClaimTypes.UserId), out int result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Return email from user claims.
        /// </summary>
        /// <param name="userClaims">All user claims.</param>
        /// <returns>User email.</returns>
        public static int GetUserEmail(this ClaimsPrincipal claimsPrincipal)
        {
            if (int.TryParse(GetValueFromUserClaims(claimsPrincipal, UserClaimTypes.Email), out int result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Return specific value from user claims.
        /// </summary>
        /// <param name="userClaims">All user claims.</param>
        /// <param name="userClaimType">Claim type.</param>
        /// <returns>Claim value.</returns>
        private static string GetValueFromUserClaims(ClaimsPrincipal claimsPrincipal, string userClaimType)
        {
            return claimsPrincipal.Claims.FirstOrDefault(x => x.Type == userClaimType)?.Value ?? string.Empty;
        }
    }
}
