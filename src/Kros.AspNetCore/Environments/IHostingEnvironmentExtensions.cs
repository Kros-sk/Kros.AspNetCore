namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// Extension for IHostingEnvironment.
    /// </summary>
    public static class IHostingEnvironmentExtensions
    {
        /// <summary>
        /// Check if current environment is Test.
        /// </summary>
        /// <param name="env">Current environment.</param>
        /// <returns>True if current environment is Test, false otherwise.</returns>
        public static bool IsTest(this IHostingEnvironment env)
            => env.IsEnvironment(Environments.Test);

        /// <summary>
        /// Check if current environment is Test or Development.
        /// </summary>
        /// <param name="env">Current environment.</param>
        /// <returns>True if current environment is Test or Development, false otherwise.</returns>
        public static bool IsTestOrDevelopment(this IHostingEnvironment env)
            => env.IsTest() || env.IsDevelopment();

        /// <summary>
        /// Check if current environment is Staging or Production.
        /// </summary>
        /// <param name="env">Current environment.</param>
        /// <returns>True if current environment is Staging or Production, false otherwise.</returns>
        public static bool IsStagingOrProduction(this IHostingEnvironment env)
            => env.IsStaging() || env.IsProduction();
    }
}
