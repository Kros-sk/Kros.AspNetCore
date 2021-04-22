using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Kros.ApplicationInsights.Extensions.Tests")]
namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Telemetry Processor to filter out requests for specific endpoints (/health).
    /// </summary>
    /// <seealso cref="ITelemetryProcessor" />
    internal class FilterRequestsProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        private readonly string[] _skippedRequests =
        {
            "/health",
            "/signalR"
        };

        private readonly string[] _skippedAgents =
        {
            "postman"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterRequestsProcessor"/> class.
        /// </summary>
        /// <param name="next">ITelemetryProcessor instance.</param>
        public FilterRequestsProcessor(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        /// <summary>
        /// Filters out requests contaning any of defined skipped requests.
        /// </summary>
        /// <param name="item">ITelemetry instance.</param>
        public void Process(ITelemetry item)
        {
            if (item is RequestTelemetry request)
            {
                string userAgent = GetUserAgentName(request);

                if (IsHttpOptions(request)
                    || _skippedRequests.Any(x => request.Name.Contains(x, StringComparison.OrdinalIgnoreCase))
                    || _skippedAgents.Any(a => userAgent.Contains(a, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
            }

            Next.Process(item);
        }

        private static bool IsHttpOptions(RequestTelemetry request)
            => request.Name.StartsWith(HttpMethods.Options, StringComparison.OrdinalIgnoreCase);

        private string GetUserAgentName(RequestTelemetry request)
         => request.Context?.User?.Id ?? string.Empty;
    }
}
