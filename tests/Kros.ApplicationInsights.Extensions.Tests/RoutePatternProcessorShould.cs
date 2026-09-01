using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class RoutePatternProcessorShould
    {
        [Fact]
        public void AddRoutePatternToTelemetry()
        {
            using TestActivity activity = new("request");

            new RoutePatternProcessor(FakeHttpContextAccessor(true)).OnEnd(activity.Activity);

            Assert.Equal("weather/{town}", activity.Activity.GetTagItem("route_pattern"));
        }

        [Fact]
        public void NoRoutePatternIfClaimIsMissing()
        {
            using TestActivity activity = new("request");

            new RoutePatternProcessor(FakeHttpContextAccessor(false)).OnEnd(activity.Activity);

            Assert.Null(activity.Activity.GetTagItem("route_pattern"));
        }

        [Fact]
        public void NoRoutePatternForDependencyTelemetry()
        {
            using TestActivity activity = new("dependency", ActivityKind.Client);

            new RoutePatternProcessor(FakeHttpContextAccessor(true)).OnEnd(activity.Activity);

            Assert.Null(activity.Activity.GetTagItem("route_pattern"));
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
