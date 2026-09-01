using Microsoft.AspNetCore.Http;
using System;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class HeadersTelemetryProcessorShould
    {
        [Fact]
        public void AddHeadersToTelemetry()
        {
            HeadersTelemetryProcessor processor = CreateProcessor(null, "Accept", "Accept-Language", "x-my-custom");
            using TestActivity activity = new("request");

            processor.OnEnd(activity.Activity);

            Assert.Equal("application/json", activity.Activity.GetTagItem("Header-Accept"));
            Assert.NotNull(activity.Activity.GetTagItem("Header-Accept-Language"));
            Assert.Equal("custom value", activity.Activity.GetTagItem("Header-x-my-custom"));
        }

        [Fact]
        public void AddOnlyDefinedHeaders()
        {
            HeadersTelemetryProcessor processor = CreateProcessor(null, "Accept-Language", "UnExistingHeader");
            using TestActivity activity = new("request");

            processor.OnEnd(activity.Activity);

            Assert.NotNull(activity.Activity.GetTagItem("Header-Accept-Language"));
            Assert.Null(activity.Activity.GetTagItem("Header-UnExistingHeader"));
            Assert.Null(activity.Activity.GetTagItem("Header-Accept"));
        }

        [Fact]
        public void AddWithCustomNames()
        {
            HeadersTelemetryProcessor processor
                = CreateProcessor((k) => $"myprefix-{k}-mysufix", "Accept-Language", "UnExistingHeader");
            using TestActivity activity = new("request");

            processor.OnEnd(activity.Activity);

            Assert.NotNull(activity.Activity.GetTagItem("myprefix-Accept-Language-mysufix"));
        }

        private static HeadersTelemetryProcessor CreateProcessor(
            Func<string, string> propertyNameResolver,
            params string[] headersToCapture)
        {
            HeadersTelemetryProcessor.HeadersToCaptureOptions option
                = new HeadersTelemetryProcessor.HeadersToCaptureOptions()
                .Add(headersToCapture);
            if (propertyNameResolver is not null)
            {
                option.PropertyNameResolver = propertyNameResolver;
            }

            return new(
                FakeHttpContextAccessor(),
                Microsoft.Extensions.Options.Options.Create(option));
        }

        private static HttpContextAccessor FakeHttpContextAccessor()
        {
            DefaultHttpContext httpContext = new();
            HttpContextAccessor context = new()
            {
                HttpContext = httpContext
            };

            context.HttpContext.Request.Headers.Append("Accept", "application/json");
            context.HttpContext.Request.Headers.Append("Accept-Language", "en-US,en;q=0.9,sk;q=0.8,cs;q=0.7");
            context.HttpContext.Request.Headers.Append("x-my-custom", "custom value");

            return context;
        }
    }
}
