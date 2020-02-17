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
        /// <param name="claimsPrincipal">Claims principal which contains all user claims.</param>
        /// <returns>User id.</returns>
        public static long GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            if (long.TryParse(claimsPrincipal.GetValueFromUserClaims(UserClaimTypes.UserId), out long result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Return email from user claims.
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal which contains all user claims.</param>
        /// <returns>User email.</returns>
        public static string GetUserEmail(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.GetValueFromUserClaims(ClaimTypes.Email);

        /// <summary>
        /// Return specific value from user claims.
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal which contains all user claims.</param>
        /// <param name="userClaimType">Claim type.</param>
        /// <returns>Claim value.</returns>
        public static string GetValueFromUserClaims(this ClaimsPrincipal claimsPrincipal, string userClaimType)
            => claimsPrincipal.Claims.FirstOrDefault(x => x.Type == userClaimType)?.Value ?? string.Empty;
    }
}
