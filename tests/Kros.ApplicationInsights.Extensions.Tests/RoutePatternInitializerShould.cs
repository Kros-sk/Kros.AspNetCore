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
            RoutePatternInitializer initializer = new(FakeHttpContextAccessor(true));
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            (telemetry as RequestTelemetry).Properties.Should().ContainKey("route_pattern");
        }

        [Fact]
        public void NoRoutePatternIfHeaderIsEmpty()
        {
            RoutePatternInitializer initializer = new(FakeHttpContextAccessor(false));
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
            DefaultHttpContext httpContext = new();
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
