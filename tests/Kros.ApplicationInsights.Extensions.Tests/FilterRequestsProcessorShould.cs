using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class FilterRequestsProcessorShould
    {
        private readonly string[] _skippedRequests =
        {
            "/health"
        };

        [Fact]
        public void PassRequestToNextIfItIsNotForbbidenRequest()
        {
            RequestTelemetry requestTelemetry = ProcessItems("someRequests");

            requestTelemetry.Sequence.Should().Be("TestPassed");
        }

        [Fact]
        public void DontPassRequestToNextIfItIsForbbidenRequest()
        {
            int passedRequests = 0;
            RequestTelemetry requestTelemetry;
            foreach (string name in _skippedRequests)
            {
                requestTelemetry = ProcessItems(name);
                if(requestTelemetry.Sequence.Equals("TestPassed"))
                {
                    passedRequests++;
                }
            }

            passedRequests.Should().Be(0);
        }

        private RequestTelemetry ProcessItems(string name)
        {
            RequestTelemetry requestTelemetry = new RequestTelemetry()
            {
                Name = name,
                Sequence = ""
            };

            PassedToNextTelemetryProcessor next = new PassedToNextTelemetryProcessor();
            FilterRequestsProcessor filterRequestsProcessor = new FilterRequestsProcessor(next);

            filterRequestsProcessor.Process(requestTelemetry);

            return requestTelemetry;
        }
    }
}
