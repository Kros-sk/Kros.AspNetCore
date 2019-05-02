namespace Kros.AspNetCore.Options
{
    /// <summary>
    /// Cors options.
    /// </summary>
    public class CorsOptions
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
