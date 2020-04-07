using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Telemetry Processor to filter out requests for specific endpoints (/health).
    /// </summary>
    /// <seealso cref="Microsoft.ApplicationInsights.Extensibility.ITelemetryProcessor" />
    class FilterRequestsProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        private readonly string[] _skippedRequests =
        {
            "/health"
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
            var request = item as RequestTelemetry;
            if(request != null && _skippedRequests.Any(x => request.Name.Contains(x)))
            {
                return;
            }

            Next.Process(item);
        }
    }
}
