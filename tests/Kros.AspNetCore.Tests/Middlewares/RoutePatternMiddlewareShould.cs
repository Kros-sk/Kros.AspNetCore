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
            DefaultHttpContext context = new();
            context.Request.Method = "POST";
            IEndpointFeature endpointFeature = Substitute.For<IEndpointFeature>();
            RoutePattern routePattern = RoutePatternFactory.Pattern("weather/{town}");
            RouteEndpoint routeEndpoint = new(_ => null, routePattern, default, default, string.Empty);
            endpointFeature.Endpoint.Returns(routeEndpoint);
            RoutePatternMiddleware middleware = new(innerHttpContext =>
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
            DefaultHttpContext context = new();
            context.Request.Method = "POST";
            RoutePatternMiddleware middleware = new(innerHttpContext =>
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
