namespace Kros.ApplicationInsights.Extensions.Options
{
    /// <summary>
    /// Settings for adaptive sampling.
    /// </summary>
    public class AdaptiveSamplingOptions
    {
        /// <summary>
        /// Maximum number of telemetry items to be generated on this application instance.
        /// </summary>
        public double MaxTelemetryItemsPerSecond { get; set; }

        /// <summary>
        /// Semicolon separated list of types that should not be sampled. Allowed type names:
        /// Dependency, Event, Exception, PageView, Request, Trace.
        /// </summary>
        public string ExcludedTypes { get; set; }

        /// <summary>
        /// Semicolon separated list of types that should be sampled. All types are sampled
        /// when left empty. Allowed type names: Dependency, Event, Exception, PageView,
        /// Request, Trace.
        /// </summary>
        public string IncludedTypes { get; set; }
    }
}
