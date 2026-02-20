using Kros.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization;

public class CachedJwtTokenProviderShould
{
    private const string JwtToken = "MyJwtToken";
    private const string CachedToken = "CachedToken";

    [Fact]
    public async Task GetJwtTokenAsync_ReturnsCachedValue_WhenAvailable()
    {
        string cacheKey = "cache_key";
        string token = "access_token";

        ICacheService cacheService = Substitute.For<ICacheService>();
        IJwtTokenProvider innerProvider = Substitute.For<IJwtTokenProvider>();
        ICacheKeyBuilder cacheKeyBuilder = Substitute.For<ICacheKeyBuilder>();

        cacheService.GetOrCreateAsync(cacheKey, Arg.Any<Func<Task<string>>>())
            .Returns(CachedToken);
        cacheKeyBuilder.BuildCacheKey(token).Returns(cacheKey);

        CachedJwtTokenProvider provider = CreateProvider(cacheService, innerProvider, cacheKeyBuilder);

        string result = await provider.GetJwtTokenAsync(token);

        Assert.Equal(CachedToken, result);
    }

    [Fact]
    public async Task GetJwtTokenForHashAsync_ReturnsCachedValue_WhenAvailable()
    {
        StringValues hashValue = "hash_value";
        string cacheKey = "hash_cache_key";

        ICacheService cacheService = Substitute.For<ICacheService>();
        IJwtTokenProvider innerProvider = Substitute.For<IJwtTokenProvider>();
        ICacheKeyBuilder cacheKeyBuilder = Substitute.For<ICacheKeyBuilder>();

        cacheService.GetOrCreateAsync(cacheKey, Arg.Any<Func<Task<string>>>())
            .Returns(CachedToken);
        cacheKeyBuilder.BuildHashCacheKey(hashValue).Returns(cacheKey);

        CachedJwtTokenProvider provider = CreateProvider(cacheService, innerProvider, cacheKeyBuilder);

        string result = await provider.GetJwtTokenForHashAsync(hashValue);

        Assert.Equal(CachedToken, result);
    }

    [Fact]
    public async Task GetJwtTokenAsync_BypassesCache_WhenCacheNotAllowed()
    {
        string token = "access_token";
        ICacheService cacheService = Substitute.For<ICacheService>();
        IJwtTokenProvider innerProvider = Substitute.For<IJwtTokenProvider>();
        ICacheKeyBuilder cacheKeyBuilder = Substitute.For<ICacheKeyBuilder>();

        innerProvider.GetJwtTokenAsync(token).Returns(JwtToken);

        GatewayJwtAuthorizationOptions options = new()
        {
            CacheSlidingExpirationOffset = TimeSpan.Zero,
            CacheAbsoluteExpiration = TimeSpan.Zero
        };

        CachedJwtTokenProvider provider = CreateProvider(cacheService, innerProvider, cacheKeyBuilder, options);

        string result = await provider.GetJwtTokenAsync(token);

        Assert.Equal(JwtToken, result);
        await cacheService.DidNotReceive().GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<string>>>());
    }

    [Fact]
    public async Task GetJwtTokenAsync_BypassesCache_WhenPathIsIgnored()
    {
        StringValues token = "access_token";

        ICacheService cacheService = Substitute.For<ICacheService>();
        IJwtTokenProvider innerProvider = Substitute.For<IJwtTokenProvider>();
        ICacheKeyBuilder cacheKeyBuilder = Substitute.For<ICacheKeyBuilder>();

        innerProvider.GetJwtTokenAsync(token).Returns(JwtToken);

        GatewayJwtAuthorizationOptions options = new()
        {
            CacheSlidingExpirationOffset = TimeSpan.FromMinutes(5)
        };
        options.IgnoredPathForCache.Add("/ignored");

        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = "/ignored";

        CachedJwtTokenProvider provider = CreateProvider(
            cacheService, innerProvider, cacheKeyBuilder, options, httpContext);

        string result = await provider.GetJwtTokenAsync(token);

        Assert.Equal(JwtToken, result);
        await cacheService.DidNotReceive().GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<string>>>());
    }

    private static CachedJwtTokenProvider CreateProvider(
        ICacheService cacheService,
        IJwtTokenProvider innerProvider,
        ICacheKeyBuilder cacheKeyBuilder,
        GatewayJwtAuthorizationOptions options = null,
        HttpContext httpContext = null)
    {
        options ??= new GatewayJwtAuthorizationOptions
        {
            CacheSlidingExpirationOffset = TimeSpan.FromMinutes(5)
        };

        httpContext ??= new DefaultHttpContext();

        IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        return new CachedJwtTokenProvider(
            cacheService,
            httpContextAccessor,
            Microsoft.Extensions.Options.Options.Create(options),
            innerProvider,
            cacheKeyBuilder);
    }
}
