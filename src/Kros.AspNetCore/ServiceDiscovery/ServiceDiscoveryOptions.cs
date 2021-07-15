namespace Kros.AspNetCore.ServiceDiscovery
{
    /// <summary>
    /// Options for <see cref="ServiceDiscoveryProvider"/>
    /// </summary>
    public class ServiceDiscoveryOptions
    {
        /// <summary>
        /// Gets or sets the name of the section where is store information about services.
        /// </summary>
        public string SectionName { get; set; } = "Services";

        /// <summary>
        /// Gets or sets a value indicating whether allow service name as host if service doesn't exist in configuration.
        /// </summary>
        public bool AllowServiceNameAsHost { get; set; } = false;

        /// <summary>
        /// Gets or sets the port. (Only if <see cref="AllowServiceNameAsHost"/> is <see langword="true"/>.)
        /// </summary>
        public int Port { get; set; } = 80;

        /// <summary>
        /// Gets or sets the scheme. (Only if <see cref="AllowServiceNameAsHost"/> is <see langword="true"/>.)
        /// </summary>
        public string Scheme { get; set; } = "http";

        /// <summary>
        /// Gets the default instance.
        /// </summary>
        public static ServiceDiscoveryOptions Default { get; } = new ServiceDiscoveryOptions();
    }
}
