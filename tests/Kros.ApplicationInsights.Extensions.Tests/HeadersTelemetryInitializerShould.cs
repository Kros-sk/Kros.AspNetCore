using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using System;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class HeadersTelemetryInitializerShould
    {
        [Fact]
        public void AddHeadersToTelemetry()
        {
            HeadersTelemetryInitializer initializer = CreateInitializer(null, "Accept", "Accept-Language", "x-my-custom");
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            Assert.Contains("Header-Accept", (telemetry as RequestTelemetry).Properties.Keys);
            Assert.Contains("Header-Accept-Language", (telemetry as RequestTelemetry).Properties.Keys);
            Assert.Contains("Header-x-my-custom", (telemetry as RequestTelemetry).Properties.Keys);
        }

        [Fact]
        public void AddOnlyDefinedHeaders()
        {
            HeadersTelemetryInitializer initializer = CreateInitializer(null, "Accept-Language", "UnExistingHeader");
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            Assert.Contains("Header-Accept-Language", (telemetry as RequestTelemetry).Properties.Keys);
        }

        [Fact]
        public void AddWithCustomNames()
        {
            HeadersTelemetryInitializer initializer = CreateInitializer((k) => $"myprefix-{k}-mysufix", "Accept-Language", "UnExistingHeader");
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            Assert.Contains("myprefix-Accept-Language-mysufix", (telemetry as RequestTelemetry).Properties.Keys);
        }

        private static HeadersTelemetryInitializer CreateInitializer(
            Func<string, string> propertyNameResolver,
            params string[] headersToCapture)
        {
            HeadersTelemetryInitializer.HeadersToCaptureOptions option
                = new HeadersTelemetryInitializer.HeadersToCaptureOptions()
                .Add(headersToCapture);
            if (propertyNameResolver is not null)
            {
                option.PropertyNameResolver = propertyNameResolver;
            }

            return new(
                FakeHttpContextAccessor(),
                Microsoft.Extensions.Options.Options.Create(option));
        }

        private static RequestTelemetry FakeTelemetry()
        {
            return new RequestTelemetry();
        }

        private static HttpContextAccessor FakeHttpContextAccessor()
        {
            DefaultHttpContext httpContext = new();
            HttpContextAccessor context = new()
            {
                HttpContext = httpContext
            };

            context.HttpContext.Request.Headers.Add("Accept", "application/json");
            context.HttpContext.Request.Headers.Add("Accept-Language", "en-US,en;q=0.9,sk;q=0.8,cs;q=0.7");
            context.HttpContext.Request.Headers.Add("x-my-custom", "custom value");

            return context;
        }
    }
}
