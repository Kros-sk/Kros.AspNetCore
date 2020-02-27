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
        /// Gets the default instance.
        /// </summary>
        public static ServiceDiscoveryOptions Default { get; } = new ServiceDiscoveryOptions();
    }
}
