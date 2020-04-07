namespace Kros.AspNetCore.Configuration
{
    /// <summary>
    /// Cors options.
    /// </summary>
    public static class CorsOptions
    {
        /// <summary>
        /// Name of cors section in appsettings.json.
        /// </summary>
        public const string CorsSectionName = "AllowedHosts";

        /// <summary>
        /// Name of custom cors policy.
        /// </summary>
        public const string CorsPolicyName = "CustomCorsPolicy";
    }
}
