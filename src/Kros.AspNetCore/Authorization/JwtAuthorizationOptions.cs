namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// JWT authorization options for authorization service.
    /// </summary>
    public class JwtAuthorizationOptions
    {
        /// <summary>
        /// Authentication service url.
        /// </summary>
        public string IdentityServerUserInfoEndpoint { get; set; }
    }
}
