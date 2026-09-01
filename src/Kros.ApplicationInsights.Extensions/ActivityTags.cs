namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Names of the OpenTelemetry attributes (activity tags) used by the Application Insights SDK.
    /// </summary>
    /// <remarks>
    /// The SDK understands both the stable HTTP semantic conventions and the older ones, so telemetry
    /// is inspected using both names. Which one is present depends on the instrumentation library
    /// emitting the activity.
    /// </remarks>
    internal static class ActivityTags
    {
        /// <summary>Request path, stable convention.</summary>
        public const string UrlPath = "url.path";

        /// <summary>Request path, legacy convention.</summary>
        public const string HttpTarget = "http.target";

        /// <summary>Route template of the matched endpoint.</summary>
        public const string HttpRoute = "http.route";

        /// <summary>HTTP method, stable convention.</summary>
        public const string HttpRequestMethod = "http.request.method";

        /// <summary>HTTP method, legacy convention.</summary>
        public const string HttpMethod = "http.method";

        /// <summary>User agent, stable convention.</summary>
        public const string UserAgentOriginal = "user_agent.original";

        /// <summary>User agent, legacy convention.</summary>
        public const string HttpUserAgent = "http.user_agent";

        /// <summary>User id. Mapped to <c>ai.user.id</c> by the Azure Monitor exporter.</summary>
        public const string EnduserId = "enduser.id";

        /// <summary>Source of a synthetic request (bot, availability test, ...).</summary>
        public const string SyntheticSource = "microsoft.synthetic_source";
    }
}
