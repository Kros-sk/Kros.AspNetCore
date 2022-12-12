using FluentAssertions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class RoutePatternInitializerShould
    {
        [Fact]
        public void AddRoutePatternToTelemetry()
        {
            var initializer = new RoutePatternInitializer(FakeHttpContextAccessor(true));
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            (telemetry as RequestTelemetry).Properties.Should().ContainKey("route_pattern");
        }

        [Fact]
        public void NoRoutePatternIfHeaderIsEmpty()
        {
            var initializer = new RoutePatternInitializer(FakeHttpContextAccessor(false));
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            (telemetry as RequestTelemetry).Properties.Should().NotContainKey("route_pattern");
        }
        private static ITelemetry FakeTelemetry()
        {
            return new RequestTelemetry();
        }

        private static IHttpContextAccessor FakeHttpContextAccessor(bool addRoutePattern)
        {
            IHeaderDictionary headers = new HeaderDictionary();
            var httpContext = new DefaultHttpContext();
            IHttpContextAccessor context = new HttpContextAccessor()
            {
                HttpContext = httpContext
            };
            if (addRoutePattern)
            {
                context.HttpContext.User.Identities.FirstOrDefault().AddClaim(new Claim("route_pattern", "weather/{town}"));
            }

            return context;
        }
    }
}
