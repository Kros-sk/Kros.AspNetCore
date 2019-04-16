using Kros.AspNetCore;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Configuration extensions.
    /// </summary>
    public static class IConfigurationExtensions
    {
        /// <summary>
        /// Get options from configuration.
        /// </summary>
        /// <typeparam name="TOptions">Options type.</typeparam>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Options.</returns>
        public static TOptions GetOptions<TOptions>(
            this IConfiguration configuration) where TOptions : class
            => configuration.GetSection(Helpers.GetSectionName<TOptions>()).Get<TOptions>();
    }
}
