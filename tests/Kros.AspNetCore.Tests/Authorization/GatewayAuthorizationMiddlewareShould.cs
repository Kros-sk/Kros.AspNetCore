using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization;

public class GatewayAuthorizationMiddlewareShould
{
    private const string UpstreamJwtToken = "upstream_jwt_token";
    private const string DownstreamJwtToken = "downstream_jwt_token";
    private const string UpstreamHashValue = "upstream_hash_value";
    private const string DownstreamHashJwtToken = "downstream_hash_jwt_token";
    private const string HashParameterName = "upstream_hash_param_name";

    [Fact]
    public async Task AddJwtTokenIntoHeader()
    {
        IJwtTokenProvider jwtProvider = Substitute.For<IJwtTokenProvider>();
        jwtProvider.GetJwtTokenAsync(UpstreamJwtToken).Returns(DownstreamJwtToken);

        GatewayAuthorizationMiddleware middleware = CreateMiddleware();
        DefaultHttpContext context = new();
        context.Request.Headers[HeaderNames.Authorization] = UpstreamJwtToken;

        await middleware.Invoke(context, jwtProvider);

        context.Request.Headers[HeaderNames.Authorization]
            .Should()
            .HaveCount(1)
            .And
            .Contain($"Bearer {DownstreamJwtToken}");
    }

    [Fact]
    public async Task AddJwtTokenIntoHeaderForHash()
    {
        IJwtTokenProvider jwtProvider = Substitute.For<IJwtTokenProvider>();
        jwtProvider.GetJwtTokenForHashAsync(UpstreamHashValue).Returns(DownstreamHashJwtToken);

        GatewayJwtAuthorizationOptions options = new() { HashParameterName = HashParameterName };
        GatewayAuthorizationMiddleware middleware = CreateMiddleware(options);
        DefaultHttpContext context = new();
        context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
        {
            { HashParameterName, UpstreamHashValue }
        });

        await middleware.Invoke(context, jwtProvider);

        context.Request.Headers[HeaderNames.Authorization]
            .Should()
            .HaveCount(1)
            .And
            .Contain($"Bearer {DownstreamHashJwtToken}");
    }

    [Fact]
    public async Task AddJwtTokenIntoHeader_WithAccessTokenPriorityOverHash()
    {
        IJwtTokenProvider jwtProvider = Substitute.For<IJwtTokenProvider>();
        jwtProvider.GetJwtTokenAsync(UpstreamJwtToken).Returns(DownstreamJwtToken);
        jwtProvider.GetJwtTokenForHashAsync(UpstreamHashValue).Returns(DownstreamHashJwtToken);

        GatewayJwtAuthorizationOptions options = new() { HashParameterName = HashParameterName };
        GatewayAuthorizationMiddleware middleware = CreateMiddleware(options);
        DefaultHttpContext context = new();
        context.Request.Headers[HeaderNames.Authorization] = UpstreamJwtToken;
        context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
        {
            { HashParameterName, UpstreamHashValue }
        });

        await middleware.Invoke(context, jwtProvider);

        context.Request.Headers[HeaderNames.Authorization]
            .Should()
            .HaveCount(1)
            .And
            .Contain($"Bearer {DownstreamJwtToken}");
    }

    [Fact]
    public async Task DoNotAddJwtTokenIntoHeaderWhenAuthorizationHeaderAndHashIsMissing()
    {
        IJwtTokenProvider jwtProvider = Substitute.For<IJwtTokenProvider>();
        GatewayAuthorizationMiddleware middleware = CreateMiddleware();
        DefaultHttpContext context = new();

        await middleware.Invoke(context, jwtProvider);

        context.Request.Headers[HeaderNames.Authorization]
            .Should().BeEmpty();
    }

    [Fact]
    public async Task CallNextMiddlewareAfterProcessing()
    {
        bool nextCalled = false;
        Task next(HttpContext context)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        IJwtTokenProvider jwtProvider = Substitute.For<IJwtTokenProvider>();
        jwtProvider.GetJwtTokenAsync(UpstreamJwtToken).Returns(DownstreamJwtToken);

        GatewayAuthorizationMiddleware middleware = new(next, new GatewayJwtAuthorizationOptions());
        DefaultHttpContext context = new();
        context.Request.Headers[HeaderNames.Authorization] = UpstreamJwtToken;

        await middleware.Invoke(context, jwtProvider);

        nextCalled.Should().BeTrue();
    }

    private static GatewayAuthorizationMiddleware CreateMiddleware(GatewayJwtAuthorizationOptions options = null)
    {
        options ??= new GatewayJwtAuthorizationOptions();
        return new GatewayAuthorizationMiddleware((context) => Task.CompletedTask, options);
    }
}
