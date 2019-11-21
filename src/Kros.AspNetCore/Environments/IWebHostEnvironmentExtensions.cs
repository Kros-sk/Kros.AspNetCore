using Microsoft.AspNetCore.Hosting;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Extension for IWebHostEnvironment.
    /// </summary>
    public static class IWebHostEnvironmentExtensions
    {
        /// <summary>
        /// Check if current environment is <see cref="EnvironmentsExtended.Test"/>.
        /// </summary>
        /// <param name="env">Current environment.</param>
        /// <returns><see langword="true" /> if current environment is <see cref="EnvironmentsExtended.Test"/>,
        /// <see langword="false" /> otherwise.</returns>
        public static bool IsTest(this IWebHostEnvironment env)
            => env.IsEnvironment(EnvironmentsExtended.Test);

        /// <summary>
        /// Check if current environment is <see cref="EnvironmentsExtended.Test"/> or <see cref="Environments.Development"/>.
        /// </summary>
        /// <param name="env">Current environment.</param>
        /// <returns><see langword="true" /> if current environment is <see cref="EnvironmentsExtended.Test"/> or
        /// <see cref="Environments.Development"/>, <see langword="false" /> otherwise.</returns>
        public static bool IsTestOrDevelopment(this IWebHostEnvironment env)
            => env.IsTest() || env.IsDevelopment();

        /// <summary>
        /// Check if current environment is <see cref="Environments.Staging"/> or <see cref="Environments.Production"/>.
        /// </summary>
        /// <param name="env">Current environment.</param>
        /// <returns><see langword="True" /> if current environment is <see cref="Environments.Staging"/> or
        /// <see cref="Environments.Production"/>, <see langword="false" /> otherwise.</returns>
        public static bool IsStagingOrProduction(this IWebHostEnvironment env)
            => env.IsStaging() || env.IsProduction();
    }
}
