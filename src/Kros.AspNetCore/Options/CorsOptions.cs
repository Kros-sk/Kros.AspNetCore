using System;

namespace Kros.AspNetCore.Options
{
    /// <summary>
    /// Cors options.
    /// </summary>
    [Obsolete("Class was moved to Kros.AspNetCore.Configuration namespace.")]
    public class CorsOptions
    {
        /// <summary>
        /// Name of cors section in appsettings.json.
        /// </summary>
        public const string CorsSectionName = Kros.AspNetCore.Configuration.CorsOptions.CorsSectionName;

        /// <summary>
        /// Name of custom cors policy.
        /// </summary>
        public const string CorsPolicyName = Kros.AspNetCore.Configuration.CorsOptions.CorsPolicyName;
    }
}
