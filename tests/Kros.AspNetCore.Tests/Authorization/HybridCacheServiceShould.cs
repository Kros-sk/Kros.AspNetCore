using Kros.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization;

public class HybridCacheServiceShould
{
    [Fact]
    public async Task GetOrCreateAsync_CallsHybridCache_WithCorrectParameters()
    {
        HybridCache mockHybridCache = Substitute.For<HybridCache>();
        const string key = "test_key";
        const string expectedValue = "test_value";
        
        mockHybridCache.GetOrCreateAsync(
            key,
            Arg.Any<Func<CancellationToken, ValueTask<string>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedValue);

        HybridCacheService service = CreateService(mockHybridCache);

        string result = await service.GetOrCreateAsync(key, () => Task.FromResult("factory_value"));

        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task GetOrCreateAsync_SetsAbsoluteExpiration_WhenConfigured()
    {
        HybridCache mockHybridCache = Substitute.For<HybridCache>();
        HybridCacheEntryOptions capturedOptions = null;

        mockHybridCache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<string>>>(),
            Arg.Do<HybridCacheEntryOptions>(options => capturedOptions = options),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>())
            .Returns("test_value");

        GatewayJwtAuthorizationOptions options = new()
        {
            CacheAbsoluteExpiration = TimeSpan.FromHours(1)
        };
        HybridCacheService service = CreateService(mockHybridCache, options);

        await service.GetOrCreateAsync("test_key", () => Task.FromResult("test_value"));

        Assert.NotNull(capturedOptions);
        Assert.Equal(TimeSpan.FromHours(1), capturedOptions.Expiration);
    }

    [Fact]
    public async Task GetOrCreateAsync_UsesSlidingExpirationAsAbsolute_WhenOnlySlidingConfigured()
    {
        HybridCache mockHybridCache = Substitute.For<HybridCache>();
        HybridCacheEntryOptions capturedOptions = null;

        mockHybridCache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<string>>>(),
            Arg.Do<HybridCacheEntryOptions>(options => capturedOptions = options),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>())
            .Returns("test_value");

        GatewayJwtAuthorizationOptions options = new()
        {
            CacheSlidingExpirationOffset = TimeSpan.FromMinutes(30)
        };
        HybridCacheService service = CreateService(mockHybridCache, options);

        await service.GetOrCreateAsync("test_key", () => Task.FromResult("test_value"));

        Assert.NotNull(capturedOptions);
        Assert.Equal(TimeSpan.FromMinutes(30), capturedOptions.Expiration);
    }

    private static HybridCacheService CreateService(
        HybridCache hybridCache,
        GatewayJwtAuthorizationOptions options = null)
    {
        options ??= new GatewayJwtAuthorizationOptions();
        
        return new HybridCacheService(
            hybridCache,
            Microsoft.Extensions.Options.Options.Create(options));
    }
}
