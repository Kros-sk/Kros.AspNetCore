using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
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

            Assert.Equal("User-Agent", (telemetry as RequestTelemetry).Context.User.Id);
        }

        [Fact]
        public void NoUserIdIfUserAgentHeaderIsEmpty()
        {
            UserIdFromUserAgentInitializer initializer = new(FakeHttpContextAccessor(false));
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            Assert.NotEqual("User-Agent", (telemetry as RequestTelemetry).Context.User.Id);
        }
        private static RequestTelemetry FakeTelemetry()
        {
            return new RequestTelemetry();
        }

        private static HttpContextAccessor FakeHttpContextAccessor(bool addUserAgent)
        {
            DefaultHttpContext httpContext = new();
            HttpContextAccessor context = new()
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
