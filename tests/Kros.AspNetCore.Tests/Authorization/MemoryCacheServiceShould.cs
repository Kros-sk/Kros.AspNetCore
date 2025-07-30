using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization;

public class MemoryCacheServiceShould
{
    [Fact]
    public async Task GetOrCreateAsync_ReturnsExistingValue_WhenKeyExists()
    {
        MemoryCache memoryCache = new(new MemoryCacheOptions());
        MemoryCacheService service = CreateService(memoryCache);
        
        const string key = "test_key";
        const string existingValue = "existing_value";
        memoryCache.Set(key, existingValue);

        bool factoryCalled = false;
        Task<string> factory()
        {
            factoryCalled = true;
            return Task.FromResult("new_value");
        }

        string result = await service.GetOrCreateAsync(key, factory);

        result.Should().Be(existingValue);
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrCreateAsync_CreatesNewValue_WhenKeyDoesNotExist()
    {
        MemoryCache memoryCache = new(new MemoryCacheOptions());
        MemoryCacheService service = CreateService(memoryCache);
        
        const string key = "test_key";
        const string newValue = "new_value";

        static Task<string> factory() => Task.FromResult(newValue);

        string result = await service.GetOrCreateAsync(key, factory);

        result.Should().Be(newValue);
        memoryCache.Get(key).Should().Be(newValue);
    }

    [Fact]
    public async Task GetOrCreateAsync_SetsExpirations_WhenConfigured()
    {
        IMemoryCache mockCache = Substitute.For<IMemoryCache>();
        ICacheEntry mockCacheEntry = Substitute.For<ICacheEntry>();
        mockCache.TryGetValue(Arg.Any<object>(), out Arg.Any<object>()).Returns(false);
        mockCache.CreateEntry(Arg.Any<object>()).Returns(mockCacheEntry);

        GatewayJwtAuthorizationOptions options = new()
        {
            CacheSlidingExpirationOffset = TimeSpan.FromMinutes(5),
            CacheAbsoluteExpiration = TimeSpan.FromHours(1)
        };
        MemoryCacheService service = CreateService(mockCache, options);
        
        await service.GetOrCreateAsync("test_key", () => Task.FromResult("test_value"));

        mockCacheEntry.Received(1).SlidingExpiration = TimeSpan.FromMinutes(5);
        mockCacheEntry.Received(1).AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
    }

    private static MemoryCacheService CreateService(
        IMemoryCache memoryCache,
        GatewayJwtAuthorizationOptions options = null)
    {
        options ??= new GatewayJwtAuthorizationOptions();
        
        return new MemoryCacheService(
            memoryCache,
            Microsoft.Extensions.Options.Options.Create(options));
    }
}
