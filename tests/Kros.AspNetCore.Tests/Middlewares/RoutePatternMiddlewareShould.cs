using FluentAssertions;
using Kros.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.Middlewares
{
    public class RoutePatternMiddlewareShould
    {
        [Fact]
        public void AddRoutePatternToClaims()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            var endpointFeature = Substitute.For<IEndpointFeature>();
            var routePattern = RoutePatternFactory.Pattern("weather/{town}");
            var routeEndpoint = new RouteEndpoint(_ => null, routePattern, default, default, string.Empty);
            endpointFeature.Endpoint.Returns(routeEndpoint);
            var middleware = new RoutePatternMiddleware(innerHttpContext =>
            {
                innerHttpContext.Features.Set(endpointFeature);
                return null;
            });

            Func<Task> action = async () => await middleware.InvokeAsync(context);

            action.Should().Throw<NullReferenceException>();
            context.
                User.
                Claims.
                FirstOrDefault(c => c.Type == "route_pattern").
                Value.
                Should().
                Be("POST weather/{town}");
        }

        [Fact]
        public void AddEmptyRoutePatternToClaims()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            var middleware = new RoutePatternMiddleware(innerHttpContext =>
            {
                return null;
            });

            Func<Task> action = async () => await middleware.InvokeAsync(context);

            action.Should().Throw<NullReferenceException>();
            context.
                User.
                Claims.
                FirstOrDefault(c => c.Type == "route_pattern").
                Value.
                Should().
                Be("POST");
        }
    }
}
