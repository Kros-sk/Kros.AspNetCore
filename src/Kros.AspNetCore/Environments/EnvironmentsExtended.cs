#if (NETCOREAPP3_0)

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Valid environments.
    /// </summary>
    public static class EnvironmentsExtended
    {
        /// <summary>
        /// Development environment.
        /// </summary>
        public static readonly string Development = Environments.Development;

        /// <summary>
        /// Test environment.
        /// </summary>
        public static readonly string Test = "Test";

        /// <summary>
        /// Staging environment.
        /// </summary>
        public static readonly string Staging = Environments.Staging;

        /// <summary>
        /// Production environment.
        /// </summary>
        public static readonly string Production = Environments.Production;
    }
}

#endif
