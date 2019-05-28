namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// User claim types.
    /// </summary>
    public static class UserClaimTypes
    {
        /// <summary>
        /// User id.
        /// </summary>
        public const string UserId = "user_id";

        /// <summary>
        /// User email.
        /// </summary>
        public const string Email = "email";

        /// <summary>
        /// Is user admin?
        /// </summary>
        public const string IsAdmin = "is_admin";

        /// <summary>
        /// User name.
        /// </summary>
        public const string GivenName = "given_name";

        /// <summary>
        /// User surname.
        /// </summary>
        public const string FamilyName = "family_name";
    }
}
