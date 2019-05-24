namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// JWT authorization options for api services.
    /// </summary>
    public class ApiJwtAuthorizationOptions
    {
        /// <summary>
        /// Secret for jwt digital sign.
        /// </summary>
        public string JwtSecret { get; set; }

        /// <summary>
        /// Https is required.
        /// </summary>
        public bool RequireHttpsMetadata { get; set; }
    }
}
