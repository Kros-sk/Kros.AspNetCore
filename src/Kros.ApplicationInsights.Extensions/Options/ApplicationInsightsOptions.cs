namespace Microsoft.Extensions.DependencyInjection.Options
{
    /// <summary>
    /// Settings for Application insights.
    /// </summary>
    public class ApplicationInsightsOptions
    {
        /// <summary>
        /// Service name, which will be display in Application map.
        /// </summary>
        public string ServiceName { get; set; }
    }
}
