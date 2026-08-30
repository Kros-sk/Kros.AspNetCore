using OpenTelemetry;
using System.Diagnostics;

namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Telemetry processor which filters out synthetic requests (bots, web search...).
    /// </summary>
    /// <seealso cref="BaseProcessor{T}" />
    internal sealed class FilterSyntheticRequestsProcessor : BaseProcessor<Activity>
    {
        /// <summary>
        /// Filters out synthetic requests.
        /// </summary>
        /// <param name="activity">Activity which has just ended.</param>
        public override void OnEnd(Activity activity)
        {
            if (activity.GetFirstTag(ActivityTags.SyntheticSource) is not null)
            {
                activity.Drop();
            }
        }
    }
}
