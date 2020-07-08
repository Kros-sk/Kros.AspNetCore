using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Kros.ApplicationInsights.Extensions.Tests")]
namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Telemetry Processor to filter out synthetic requests(bots, web search...).
    /// </summary>
    /// <seealso cref="Microsoft.ApplicationInsights.Extensibility.ITelemetryProcessor" />
    internal class FilterSyntheticRequestsProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterRequestsProcessor"/> class.
        /// </summary>
        /// <param name="next">ITelemetryProcessor instance.</param>
        public FilterSyntheticRequestsProcessor(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        /// <summary>
        /// Filters out synthethic requests.
        /// </summary>
        /// <param name="item">ITelemetry instance.</param>
        public void Process(ITelemetry item)
        {
            if (!string.IsNullOrEmpty(item.Context.Operation.SyntheticSource))
            {
                return;
            }

            Next.Process(item);
        }
    }
}
