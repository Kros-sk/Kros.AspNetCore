using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DataContracts;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Kros.ApplicationInsights.Extensions.Tests")]
namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Telemetry Processor to filter out sensitive data.
    /// </summary>
    /// <seealso cref="ITelemetryProcessor" />
    public class FilterSensitiveTraceTelemetryProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        private static readonly List<string> _sensitivePatterns = new()
        {
            "Authorization:",
            "Basic",
            "Bearer"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterSensitiveTraceTelemetryProcessor"/> class.
        /// </summary>
        /// <param name="next">ITelemetryProcessor instance.</param>
        public FilterSensitiveTraceTelemetryProcessor(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        /// <summary>
        /// Filters out sensitive data.
        /// </summary>
        /// <param name="item">ITelemetry instance.</param>
        public void Process(ITelemetry item)
        {
            if (!OKtoSend(item))
            {
                return;
            }

            this.Next.Process(item);
        }

        private bool OKtoSend(ITelemetry item)
        {
            if (item is TraceTelemetry trace)
            {
                string message = trace.Message ?? string.Empty;

                return !_sensitivePatterns.Any(p => message.Contains(p, System.StringComparison.OrdinalIgnoreCase));
            }

            return true;
        }
    }
}
