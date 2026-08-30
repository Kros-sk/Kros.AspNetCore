using System.Diagnostics;

namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Helpers for working with <see cref="Activity"/> in telemetry processors.
    /// </summary>
    internal static class ActivityExtensions
    {
        /// <summary>
        /// Marks the activity so that it is not exported to Application Insights, while keeping
        /// the trace context intact so distributed tracing still propagates to downstream services.
        /// </summary>
        /// <param name="activity">Activity to drop.</param>
        /// <remarks>
        /// Both flags are cleared on purpose. <see cref="Activity.IsAllDataRequested"/> is the mechanism
        /// documented by the Application Insights SDK, while the <see cref="ActivityTraceFlags.Recorded"/>
        /// flag is what the OpenTelemetry export processors check before handing data to an exporter.
        /// </remarks>
        public static void Drop(this Activity activity)
        {
            activity.IsAllDataRequested = false;
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }

        /// <summary>
        /// Returns the value of the first tag present from <paramref name="tagNames"/>.
        /// </summary>
        /// <param name="activity">Activity to read the tag from.</param>
        /// <param name="tagNames">Tag names to probe, in order of preference.</param>
        /// <returns>Tag value, or <see langword="null"/> when none of the tags is present.</returns>
        public static string GetFirstTag(this Activity activity, params string[] tagNames)
        {
            foreach (string tagName in tagNames)
            {
                if (activity.GetTagItem(tagName) is object value)
                {
                    string stringValue = value.ToString();
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        return stringValue;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the activity represents an incoming request.
        /// </summary>
        /// <param name="activity">Activity to check.</param>
        public static bool IsRequest(this Activity activity)
            => activity.Kind is ActivityKind.Server or ActivityKind.Consumer;
    }
}
