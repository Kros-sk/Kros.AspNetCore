using Microsoft.AspNetCore.Http;
using OpenTelemetry;
using System;
using System.Diagnostics;

namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Telemetry processor which filters out requests for specific endpoints (/health).
    /// </summary>
    /// <seealso cref="BaseProcessor{T}" />
    internal sealed class FilterRequestsProcessor : BaseProcessor<Activity>
    {
        private static readonly string[] _skippedRequests =
        {
            "/health",
            "/signalR"
        };

        private static readonly string[] _skippedAgents =
        {
            "postman"
        };

        /// <summary>
        /// Filters out requests containing any of the defined skipped requests.
        /// </summary>
        /// <param name="activity">Activity which has just ended.</param>
        public override void OnEnd(Activity activity)
        {
            if (activity.IsRequest() && ShouldSkip(activity))
            {
                activity.Drop();
            }
        }

        private static bool ShouldSkip(Activity activity)
            => IsHttpOptions(activity) || IsSkippedRequest(activity) || IsSkippedAgent(activity);

        private static bool IsHttpOptions(Activity activity)
        {
            string method = activity.GetFirstTag(ActivityTags.HttpRequestMethod, ActivityTags.HttpMethod);

            return method is not null
                ? method.Equals(HttpMethods.Options, StringComparison.OrdinalIgnoreCase)
                : activity.DisplayName.StartsWith(HttpMethods.Options, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSkippedRequest(Activity activity)
        {
            string path = activity.GetFirstTag(ActivityTags.UrlPath, ActivityTags.HttpTarget, ActivityTags.HttpRoute);

            return Contains(path, _skippedRequests) || Contains(activity.DisplayName, _skippedRequests);
        }

        private static bool IsSkippedAgent(Activity activity)
            => Contains(
                activity.GetFirstTag(ActivityTags.UserAgentOriginal, ActivityTags.HttpUserAgent),
                _skippedAgents);

        private static bool Contains(string value, string[] items)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (string item in items)
            {
                if (value.Contains(item, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
