namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// JWT authorization options for api services. Contains one authorization scheme.
    /// </summary>
    public class ApiJwtAuthorizationScheme
    {
        /// <summary>
        /// Scheme name.
        /// </summary>
        public string SchemeName { get; set; }

        /// <summary>
        /// Secret for jwt digital sign.
        /// </summary>
        public string JwtSecret { get; set; }

        /// <summary>
        /// Https is required. Default value is <see langword="true"/>.
        /// </summary>
        public bool RequireHttpsMetadata { get; set; } = true;
    }
}
