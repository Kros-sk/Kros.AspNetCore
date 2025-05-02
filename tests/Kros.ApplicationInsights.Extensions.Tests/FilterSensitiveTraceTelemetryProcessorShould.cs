using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Kros.ApplicationInsights.Extensions;
using Xunit;
using Microsoft.ApplicationInsights.Extensibility;
using FluentAssertions;

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

            next.ReceivedTelemetry.Equals(trace);
        }

        [Theory]
        [InlineData("Authorization: SecretKey")]
        [InlineData("Bearer token123")]
        [InlineData("Basic abcdefg")]
        public void FilterOutTelemetry_WhenMessageContainsSensitiveData(string sensitiveMessage)
        {
            var next = new ReceivingTestProcessor();
            var processor = new FilterSensitiveTraceTelemetryProcessor(next);
            var trace = new TraceTelemetry(sensitiveMessage);

            processor.Process(trace);

            next.ReceivedTelemetry.Should().BeNull();
        }

        [Fact]
        public void PassNonTraceTelemetry()
        {
            var next = new ReceivingTestProcessor();
            var processor = new FilterSensitiveTraceTelemetryProcessor(next);
            var eventTelemetry = new EventTelemetry("TestEvent");

            processor.Process(eventTelemetry);

            next.ReceivedTelemetry.Should().Be(eventTelemetry);
        }
    }
}
