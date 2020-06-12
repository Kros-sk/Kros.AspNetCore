using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    internal class PassedToNextTelemetryProcessor : ITelemetryProcessor
    {
        public PassedToNextTelemetryProcessor()
        {
        }

        /// <summary>
        /// Filters out synthethic requests.
        /// </summary>
        /// <param name="item">ITelemetry instance.</param>
        public void Process(ITelemetry item)
        {
            item.Sequence = "TestPassed";
        }
    }
}
