using Microsoft.ApplicationInsights.DataContracts;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class FilterSyntheticRequestsProcesorShould
    {
        [Fact]
        public void PassRequestToNextIfItIsNotSynthetic()
        {
            RequestTelemetry requestTelemetry = ProcessItems(false);

            Assert.Equal("TestPassed", requestTelemetry.Sequence);
        }

        [Fact]
        public void DontPassRequestToNextIfItIsSynthetic()
        {
            RequestTelemetry requestTelemetry = ProcessItems(true);

            Assert.NotEqual("TestPassed", requestTelemetry.Sequence);
        }

        private static RequestTelemetry ProcessItems(bool isSynthetic)
        {
            RequestTelemetry requestTelemetry = new()
            {
                Name = "SomeRequest",
                Sequence = "",
            };
            requestTelemetry.Context.Operation.SyntheticSource = isSynthetic ? "source" : null;
            PassedToNextTelemetryProcessor next = new();
            FilterSyntheticRequestsProcessor filterRequestsProcessor = new(next);

            filterRequestsProcessor.Process(requestTelemetry);

            return requestTelemetry;
        }
    }
}
