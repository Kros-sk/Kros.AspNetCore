using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class FilterRequestsProcessorShould
    {
        private readonly string[] _skippedRequests =
        {
            "/health"
        };

        private readonly string[] _skippedAgents =
        {
            "postman"
        };

        [Fact]
        public void PassRequestToNextIfItIsNotForbbidenRequest()
        {
            RequestTelemetry requestTelemetry = ProcessItems("someRequests", "NotMatter");

            requestTelemetry.Sequence.Should().Be("TestPassed");
        }

        [Fact]
        public void DontPassRequestToNextIfItIsForbbidenRequest()
        {
            int passedRequests = 0;
            RequestTelemetry requestTelemetry;
            foreach (string name in _skippedRequests)
            {
                requestTelemetry = ProcessItems(name, "NotMatter");
                if (requestTelemetry.Sequence.Equals("TestPassed"))
                {
                    passedRequests++;
                }
            }

            passedRequests.Should().Be(0);
        }

        [Fact]
        public void PassRequestToNextIfItIsNotForbbidenUserAgent()
        {
            RequestTelemetry requestTelemetry = ProcessItems("someRequests", "Safari/4.23");

            requestTelemetry.Sequence.Should().Be("TestPassed");
        }

        [Fact]
        public void DontPassRequestToNextIfItIsForbbidenUserAgent()
        {
            int passedRequests = 0;
            RequestTelemetry requestTelemetry;
            foreach (string agentName in _skippedAgents)
            {
                requestTelemetry = ProcessItems("someRequests", agentName);
                if (requestTelemetry.Sequence.Equals("TestPassed"))
                {
                    passedRequests++;
                }
            }

            passedRequests.Should().Be(0);
        }

        private RequestTelemetry ProcessItems(string name, string agentName)
        {
            RequestTelemetry requestTelemetry = new RequestTelemetry()
            {
                Name = name,
                Sequence = ""
            };
            requestTelemetry.Context.User.Id = agentName;

            PassedToNextTelemetryProcessor next = new PassedToNextTelemetryProcessor();
            FilterRequestsProcessor filterRequestsProcessor = new FilterRequestsProcessor(next);

            filterRequestsProcessor.Process(requestTelemetry);

            return requestTelemetry;
        }
    }
}
