using FluentAssertions;
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

            (telemetry as RequestTelemetry)
                .Properties
                .Should()
                .ContainKeys("Header-Accept", "Header-Accept-Language", "Header-x-my-custom");
        }

        [Fact]
        public void AddOnlyDefinedHeaders()
        {
            HeadersTelemetryInitializer initializer = CreateInitializer(null, "Accept-Language", "UnExistingHeader");
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            (telemetry as RequestTelemetry)
                .Properties
                .Should()
                .ContainKeys("Header-Accept-Language");
        }

        [Fact]
        public void AddWithCustomNames()
        {
            HeadersTelemetryInitializer initializer = CreateInitializer((k) => $"myprefix-{k}-mysufix", "Accept-Language", "UnExistingHeader");
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            (telemetry as RequestTelemetry)
                .Properties
                .Should()
                .ContainKeys("myprefix-Accept-Language-mysufix");
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

        private static ITelemetry FakeTelemetry()
        {
            return new RequestTelemetry();
        }

        private static IHttpContextAccessor FakeHttpContextAccessor()
        {
            DefaultHttpContext httpContext = new();
            IHttpContextAccessor context = new HttpContextAccessor()
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
