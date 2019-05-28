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
        /// <param name="claimsPrincipal">Claims principal which contains all user claims.</param>
        /// <returns>User email.</returns>
        public static string GetUserEmail(this ClaimsPrincipal claimsPrincipal)
            => GetValueFromUserClaims(claimsPrincipal, UserClaimTypes.Email);

        /// <summary>
        /// Return specific value from user claims.
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal which contains all user claims.</param>
        /// <param name="userClaimType">Claim type.</param>
        /// <returns>Claim value.</returns>
        private static string GetValueFromUserClaims(ClaimsPrincipal claimsPrincipal, string userClaimType)
            => claimsPrincipal.Claims.FirstOrDefault(x => x.Type == userClaimType)?.Value ?? string.Empty;
    }
}
