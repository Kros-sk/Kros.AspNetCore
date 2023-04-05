using Kros.ApplicationInsights.Extensions.Options;

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

        /// <summary>
        /// Sampling rate for fixed rate sampling.
        /// Must be a number 100/N, where N is integer.(100,50,25,...,0.1....)
        /// </summary>
        public int SamplingRate { get; set; }

        /// <summary>
        /// Adaptive sampling options.
        /// </summary>
        public AdaptiveSamplingOptions AdaptiveSamplingOptions { get; set; }
    }
}
