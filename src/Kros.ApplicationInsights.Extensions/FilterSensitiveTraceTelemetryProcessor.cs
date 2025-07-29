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
        private readonly ITelemetryProcessor _next;

        private static readonly List<string> _sensitivePatterns = new()
        {
            "Request Headers:\r\nAuthorization:",
            "Request Headers:\r\nx-functions-key:"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterSensitiveTraceTelemetryProcessor"/> class.
        /// </summary>
        /// <param name="next">ITelemetryProcessor instance.</param>
        public FilterSensitiveTraceTelemetryProcessor(ITelemetryProcessor next)
        {
            _next = next;
        }

        /// <summary>
        /// Filters out sensitive data.
        /// </summary>
        /// <param name="item">ITelemetry instance.</param>
        public void Process(ITelemetry item)
        {
            if (!OkToSend(item))
            {
                return;
            }

            _next.Process(item);
        }

        private bool OkToSend(ITelemetry item)
        {
            if (item is TraceTelemetry trace && !string.IsNullOrEmpty(trace.Message))
            {
                return !_sensitivePatterns.Any(p => trace.Message.StartsWith(p, System.StringComparison.OrdinalIgnoreCase));
            }

            return true;
        }
    }
}
