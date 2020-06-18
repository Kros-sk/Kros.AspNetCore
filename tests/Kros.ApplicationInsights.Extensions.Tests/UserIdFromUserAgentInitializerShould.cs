using FluentAssertions;
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
            UserIdFromUserAgentInitializer initializer = new UserIdFromUserAgentInitializer(FakeHttpContextAccessor(true));
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            (telemetry as RequestTelemetry).Context.User.Id.Should().Be("User-Agent");
        }

        [Fact]
        public void NoUserIdIfUserAgentHeaderIsEmpty()
        {
            UserIdFromUserAgentInitializer initializer = new UserIdFromUserAgentInitializer(FakeHttpContextAccessor(false));
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            (telemetry as RequestTelemetry).Context.User.Id.Should().NotBe("User-Agent");
        }

        private ITelemetry FakeTelemetry()
        {
            return new RequestTelemetry();
        }

        private IHttpContextAccessor FakeHttpContextAccessor(bool addUserAgent)
        {
            IHeaderDictionary headers = new HeaderDictionary();
            var httpContext = new DefaultHttpContext();
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
