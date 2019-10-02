namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// Extension for IHostingEnvironment.
    /// </summary>
    public static class IHostingEnvironmentExtensions
    {
        /// <summary>
        /// Check if current environment is <see cref="Environments.Test"/>.
        /// </summary>
        /// <param name="env">Current environment.</param>
        /// <returns><see langword="true" /> if current environment is <see cref="Environments.Test"/>,
        /// <see langword="false" /> otherwise.</returns>
        public static bool IsTest(this IHostingEnvironment env)
            => env.IsEnvironment(Environments.Test);

        /// <summary>
        /// Check if current environment is <see cref="Environments.Test"/> or <see cref="Environments.Development"/>.
        /// </summary>
        /// <param name="env">Current environment.</param>
        /// <returns><see langword="true" /> if current environment is <see cref="Environments.Test"/> or
        /// <see cref="Environments.Development"/>, <see langword="false" /> otherwise.</returns>
        public static bool IsTestOrDevelopment(this IHostingEnvironment env)
            => env.IsTest() || env.IsDevelopment();

        /// <summary>
        /// Check if current environment is <see cref="Environments.Staging"/> or <see cref="Environments.Production"/>.
        /// </summary>
        /// <param name="env">Current environment.</param>
        /// <returns><see langword="True" /> if current environment is <see cref="Environments.Staging"/> or
        /// <see cref="Environments.Production"/>, <see langword="false" /> otherwise.</returns>
        public static bool IsStagingOrProduction(this IHostingEnvironment env)
            => env.IsStaging() || env.IsProduction();
    }
}
