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

            Assert.Contains("route_pattern", (telemetry as RequestTelemetry).Properties.Keys);
        }

        [Fact]
        public void NoRoutePatternIfHeaderIsEmpty()
        {
            RoutePatternInitializer initializer = new(FakeHttpContextAccessor(false));
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            Assert.DoesNotContain("route_pattern", (telemetry as RequestTelemetry).Properties.Keys);
        }
        private static RequestTelemetry FakeTelemetry()
        {
            return new RequestTelemetry();
        }

        private static HttpContextAccessor FakeHttpContextAccessor(bool addRoutePattern)
        {
            DefaultHttpContext httpContext = new();
            HttpContextAccessor context = new()
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
