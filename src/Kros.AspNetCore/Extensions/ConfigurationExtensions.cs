using Kros.AspNetCore;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Configuration extensions.
    /// </summary>
    public static class IConfigurationExtensions
    {
        /// <summary>
        /// Get section from configuration materialized to <typeparamref name="T"/>.
        /// Section name is taken as class <typeparamref name="T"/> name without Options suffix.
        /// </summary>
        /// <typeparam name="T">The type into which the section is to materialize.</typeparam>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Section.</returns>
        public static T GetSection<T>(this IConfiguration configuration) where T : class
            => configuration.GetSection<T>(Helpers.GetSectionName<T>());

        /// <summary>
        /// Get section from configuration materialized to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type into which the section is to materialize.</typeparam>
        /// <param name="configuration">Configuration.</param>
        /// <param name="sectioName">Section name.</param>
        /// <returns>Section.</returns>
        public static T GetSection<T>(this IConfiguration configuration, string sectioName) where T : class
            => configuration.GetSection(sectioName).Get<T>();
    }
}
