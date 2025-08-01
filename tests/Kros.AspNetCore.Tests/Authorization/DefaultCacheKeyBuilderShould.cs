using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization;

public class DefaultCacheKeyBuilderShould
{
    private const string TestToken = "test_token";
    private const string TestCacheKeyPrefix = "testPrefix";
    private const string TestConnectionId = "connection_123";

    [Fact]
    public void BuildCacheKey_ReturnsCorrectFormat_WithTokenOnly()
    {
        GatewayJwtAuthorizationOptions options = CreateOptions();
        DefaultCacheKeyBuilder builder = CreateBuilder(options);

        string result = builder.BuildCacheKey(TestToken);

        result.Should().Be($"{TestCacheKeyPrefix}:v1:{GetExpectedHashForToken(TestToken, null)}");
    }

    [Fact]
    public void BuildCacheKey_IncludesCacheHeaders_WhenConfigured()
    {
        GatewayJwtAuthorizationOptions options = CreateOptions(cacheKeyHttpHeaders: ["Connection-Id"]);
        DefaultHttpContext httpContext = new();
        httpContext.Request.Headers["Connection-Id"] = TestConnectionId;
        DefaultCacheKeyBuilder builder = CreateBuilder(options, httpContext);

        string result = builder.BuildCacheKey(TestToken);

        string expectedCacheKeyPart = $"Connection-Id:{TestConnectionId}|";
        result.Should().Be($"{TestCacheKeyPrefix}:v1:{GetExpectedHashForToken(TestToken, expectedCacheKeyPart)}");
    }

    [Fact]
    public void BuildCacheKey_IncludesBothHeadersAndUrlPath_WhenBothConfigured()
    {
        GatewayJwtAuthorizationOptions options = CreateOptions(
            cacheKeyHttpHeaders: ["Connection-Id"],
            cacheKeyUrlPathRegexPattern: @"/companies/(?<companyId>\d+)");
        DefaultHttpContext httpContext = new();
        httpContext.Request.Headers["Connection-Id"] = TestConnectionId;
        httpContext.Request.Path = "/companies/789012";
        DefaultCacheKeyBuilder builder = CreateBuilder(options, httpContext);

        string result = builder.BuildCacheKey(TestToken);

        string expectedCacheKeyPart = $"Connection-Id:{TestConnectionId}|789012";
        result.Should().Be($"{TestCacheKeyPrefix}:v1:{GetExpectedHashForToken(TestToken, expectedCacheKeyPart)}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("connection_id")]
    public void BuildCacheKey_HandlesDifferentConnectionIdValues(string connectionId)
    {
        GatewayJwtAuthorizationOptions options = CreateOptions(cacheKeyHttpHeaders: ["Connection-Id"]);
        DefaultHttpContext httpContext = new();
        if (connectionId != null)
        {
            httpContext.Request.Headers["Connection-Id"] = connectionId;
        }
        DefaultCacheKeyBuilder builder = CreateBuilder(options, httpContext);

        string result = builder.BuildCacheKey(TestToken);

        result.Should().StartWith($"{TestCacheKeyPrefix}:v1:");
    }

    [Fact]
    public void BuildCacheKey_IgnoresNonConfiguredHeaders()
    {
        GatewayJwtAuthorizationOptions options = CreateOptions(cacheKeyHttpHeaders: ["Connection-Id"]);
        DefaultHttpContext httpContext = new();
        httpContext.Request.Headers["Connection-Id"] = TestConnectionId;
        httpContext.Request.Headers["Other-Header"] = "other_value";
        DefaultCacheKeyBuilder builder = CreateBuilder(options, httpContext);

        string result = builder.BuildCacheKey(TestToken);

        string expectedCacheKeyPart = $"Connection-Id:{TestConnectionId}|";
        result.Should().Be($"{TestCacheKeyPrefix}:v1:{GetExpectedHashForToken(TestToken, expectedCacheKeyPart)}");
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("", "", null)]
    [InlineData("/companies/123456", "/companies/(?<companyId>\\d+)", "123456")]
    [InlineData("/companies/123456789/other-segment", "/companies/(?<companyId>\\d+)/other-segment", "123456789")]
    [InlineData("/companies/123456", null, null)]
    [InlineData("", "/companies/(?<companyId>\\d+)", null)]
    public void BuildCacheKey_HandlesUrlPathRegexCorrectly(
        string requestPath,
        string urlPathRegexPattern,
        string expectedExtractedValue)
    {
        GatewayJwtAuthorizationOptions options = CreateOptions(cacheKeyUrlPathRegexPattern: urlPathRegexPattern);
        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = requestPath ?? "";
        DefaultCacheKeyBuilder builder = CreateBuilder(options, httpContext);

        string result = builder.BuildCacheKey(TestToken);

        result.Should().Be($"{TestCacheKeyPrefix}:v1:{GetExpectedHashForToken(TestToken, expectedExtractedValue)}");
    }

    private static DefaultCacheKeyBuilder CreateBuilder(
        GatewayJwtAuthorizationOptions options,
        HttpContext httpContext = null)
    {
        IOptions<GatewayJwtAuthorizationOptions> optionsWrapper = Microsoft.Extensions.Options.Options.Create(options);
        IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext ?? new DefaultHttpContext());
        
        return new DefaultCacheKeyBuilder(optionsWrapper, httpContextAccessor);
    }

    private static GatewayJwtAuthorizationOptions CreateOptions(
        List<string> cacheKeyHttpHeaders = null,
        string cacheKeyUrlPathRegexPattern = null,
        string cacheKeyPrefix = TestCacheKeyPrefix)
        => new()
        {
            CacheKeyHttpHeaders = cacheKeyHttpHeaders ?? [],
            CacheKeyUrlPathRegexPattern = cacheKeyUrlPathRegexPattern,
            CacheKeyPrefix = cacheKeyPrefix
        };

    private static string GetExpectedHashForToken(string token, string additionalKeyPart)
    {
        string input = additionalKeyPart is null ? token : $"{token}:{additionalKeyPart}";
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        string hash = Convert.ToBase64String(hashBytes);
        return hash;
    }
}
