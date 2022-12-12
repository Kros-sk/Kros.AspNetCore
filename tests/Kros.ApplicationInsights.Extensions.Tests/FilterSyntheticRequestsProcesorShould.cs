using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class FilterSyntheticRequestsProcesorShould
    {
        [Fact]
        public void PassRequestToNextIfItIsNotSynthetic()
        {
            RequestTelemetry requestTelemetry = ProcessItems(false);

            requestTelemetry.Sequence.Should().Be("TestPassed");
        }

        [Fact]
        public void DontPassRequestToNextIfItIsSynthetic()
        {
            RequestTelemetry requestTelemetry = ProcessItems(true);

            requestTelemetry.Sequence.Should().NotBe("TestPassed");
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
