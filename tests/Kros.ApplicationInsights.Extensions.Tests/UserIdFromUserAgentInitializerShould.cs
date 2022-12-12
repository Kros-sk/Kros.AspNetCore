using FluentAssertions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using System;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class UserIdFromUserAgentInitializerShould
    {
        [Fact]
        public void AddUserIdFromAgentHeaderToTelemetry()
        {
            UserIdFromUserAgentInitializer initializer = new(FakeHttpContextAccessor(true));
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            (telemetry as RequestTelemetry).Context.User.Id.Should().Be("User-Agent");
        }

        [Fact]
        public void NoUserIdIfUserAgentHeaderIsEmpty()
        {
            UserIdFromUserAgentInitializer initializer = new(FakeHttpContextAccessor(false));
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            (telemetry as RequestTelemetry).Context.User.Id.Should().NotBe("User-Agent");
        }
        private static ITelemetry FakeTelemetry()
        {
            return new RequestTelemetry();
        }

        private static IHttpContextAccessor FakeHttpContextAccessor(bool addUserAgent)
        {
            DefaultHttpContext httpContext = new();
            IHttpContextAccessor context = new HttpContextAccessor()
            {
                HttpContext = httpContext
            };
            if (addUserAgent)
            {
                context.HttpContext.Request.Headers.Add("User-Agent", "User-Agent");
            }

            return context;
        }
    }
}
