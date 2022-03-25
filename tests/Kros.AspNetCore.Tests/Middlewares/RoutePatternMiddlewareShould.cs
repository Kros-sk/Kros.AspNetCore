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
        public async Task AddRoutePatternToClaims()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            IEndpointFeature endpointFeature = Substitute.For<IEndpointFeature>();
            RoutePattern routePattern = RoutePatternFactory.Pattern("weather/{town}");
            var routeEndpoint = new RouteEndpoint(_ => null, routePattern, default, default, string.Empty);
            endpointFeature.Endpoint.Returns(routeEndpoint);
            var middleware = new RoutePatternMiddleware(innerHttpContext =>
            {
                innerHttpContext.Features.Set(endpointFeature);
                return null;
            });

            Func<Task> action = async () => await middleware.InvokeAsync(context);

            await action.Should().ThrowAsync<NullReferenceException>();
            context.
                User.
                Claims.
                FirstOrDefault(c => c.Type == "route_pattern").
                Value.
                Should().
                Be("POST weather/{town}");
        }

        [Fact]
        public async Task AddEmptyRoutePatternToClaims()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            var middleware = new RoutePatternMiddleware(innerHttpContext =>
            {
                return null;
            });

            Func<Task> action = async () => await middleware.InvokeAsync(context);

            await action.Should().ThrowAsync<NullReferenceException>();
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
