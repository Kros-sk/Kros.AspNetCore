using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class FilterRequestsProcessorShould
    {
        [Theory]
        [InlineData("/health", "")]
        [InlineData("/signalR", "")]
        [InlineData("OPTIONS/", "")]
        [InlineData("OPTIONS/someRequest", "")]
        [InlineData("GET/someRequest", "TestPassed")]
        [InlineData("GET/optionsrequest", "TestPassed")]
        [InlineData("someRequests", "TestPassed")]
        public void ReturnCorrectSequenceForRequestName(string name, string expectedSequence)
        {
            RequestTelemetry requestTelemetry = ProcessItems(name, "NotMatter");
            requestTelemetry.Sequence.Should().Be(expectedSequence);
        }

        [Theory]
        [InlineData("postman", "")]
        [InlineData("postman/7.5", "")]
        [InlineData("Safari", "TestPassed")]
        [InlineData("Safari/4.23", "TestPassed")]
        [InlineData("Chrome/4.23", "TestPassed")]
        [InlineData("Opera/4.23", "TestPassed")]
        public void ReturnCorrectSequenceForUserAgent(string agentName, string expectedSequence)
        {
            RequestTelemetry requestTelemetry = ProcessItems("someRequests", agentName);
            requestTelemetry.Sequence.Should().Be(expectedSequence);
        }

        private static RequestTelemetry ProcessItems(string name, string agentName)
        {
            RequestTelemetry requestTelemetry = new()
            {
                Name = name,
                Sequence = ""
            };
            requestTelemetry.Context.User.Id = agentName;

            PassedToNextTelemetryProcessor next = new();
            FilterRequestsProcessor filterRequestsProcessor = new(next);

            filterRequestsProcessor.Process(requestTelemetry);

            return requestTelemetry;
        }
    }
}
