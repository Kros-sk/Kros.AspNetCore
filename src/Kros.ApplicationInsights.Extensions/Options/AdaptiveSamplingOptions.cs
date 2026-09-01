using System;

namespace Kros.ApplicationInsights.Extensions.Options
{
    /// <summary>
    /// Settings for rate limited sampling.
    /// </summary>
    public class AdaptiveSamplingOptions
    {
        /// <summary>
        /// Maximum number of telemetry items to be generated on this application instance.
        /// </summary>
        public double MaxTelemetryItemsPerSecond { get; set; }

        /// <summary>
        /// Semicolon separated list of types that should not be sampled.
        /// </summary>
        [Obsolete("Per telemetry type sampling is not supported by Application Insights SDK 3.x. "
            + "The value is ignored and the setting will be removed in a future version.")]
        public string ExcludedTypes { get; set; }

        /// <summary>
        /// Semicolon separated list of types that should be sampled.
        /// </summary>
        [Obsolete("Per telemetry type sampling is not supported by Application Insights SDK 3.x. "
            + "The value is ignored and the setting will be removed in a future version.")]
        public string IncludedTypes { get; set; }
    }
}
