using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class FilterSensitiveTraceTelemetryProcessorShould
    {
        private class ReceivingTestProcessor : ITelemetryProcessor
        {
            public ITelemetry ReceivedTelemetry { get; private set; }

            public void Process(ITelemetry item)
            {
                ReceivedTelemetry = item;
            }
        }

        [Fact]
        public void PassTelemetry_WhenMessageDoesNotContainSensitiveData()
        {
            var next = new ReceivingTestProcessor();
            var processor = new FilterSensitiveTraceTelemetryProcessor(next);
            var trace = new TraceTelemetry("This is a harmless message.");

            processor.Process(trace);

            Assert.Same(trace, next.ReceivedTelemetry);
        }

        [Theory]
        [InlineData("Request Headers:\r\nAuthorization: SecretKey")]
        [InlineData("Request Headers:\r\nx-functions-key: SecretKey")]
        public void FilterOutTelemetry_WhenMessageContainsSensitiveData(string sensitiveMessage)
        {
            var next = new ReceivingTestProcessor();
            var processor = new FilterSensitiveTraceTelemetryProcessor(next);
            var trace = new TraceTelemetry(sensitiveMessage);

            processor.Process(trace);

            Assert.Null(next.ReceivedTelemetry);
        }

        [Fact]
        public void PassNonTraceTelemetry()
        {
            var next = new ReceivingTestProcessor();
            var processor = new FilterSensitiveTraceTelemetryProcessor(next);
            var eventTelemetry = new EventTelemetry("TestEvent");

            processor.Process(eventTelemetry);

            Assert.Same(eventTelemetry, next.ReceivedTelemetry);
        }
    }
}
