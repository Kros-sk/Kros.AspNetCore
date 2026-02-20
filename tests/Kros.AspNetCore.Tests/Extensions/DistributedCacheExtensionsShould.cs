using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class DistributedCacheExtensionsShould
    {
        #region Test class

        class Foo
        {
            public int Value { get; set; }
        }

        #endregion

        [Fact]
        public async Task SetValueToCache()
        {
            IDistributedCache cache = new MemoryDistributedCache();

            await cache.SetAsync("2", 2, new DistributedCacheEntryOptions(), TestContext.Current.CancellationToken);

            int value = await cache.GetAsync<int>("2", TestContext.Current.CancellationToken);

            Assert.Equal(2, value);
        }

        [Fact]
        public async Task SetComplexValueToCache()
        {
            IDistributedCache cache = new MemoryDistributedCache();

            await cache.SetAsync("foo", new Foo() { Value = 2 }, new DistributedCacheEntryOptions(), TestContext.Current.CancellationToken);

            Foo value = await cache.GetAsync<Foo>("foo", TestContext.Current.CancellationToken);

            Assert.Equal(2, value.Value);
        }

        [Fact]
        public async Task GetDefaultWhenKeyDoesNotExist()
        {
            IDistributedCache cache = new MemoryDistributedCache();

            Foo foo = await cache.GetAsync<Foo>("foo", TestContext.Current.CancellationToken);
            Assert.Null(foo);

            int? intValue = await cache.GetAsync<int?>("int", TestContext.Current.CancellationToken);
            Assert.Null(intValue);

            string stringValue = await cache.GetAsync<string>("string", TestContext.Current.CancellationToken);
            Assert.Null(stringValue);
        }

        [Fact]
        public async Task GetAndSetIfDoNotExistInCache()
        {
            IDistributedCache cache = new MemoryDistributedCache();
            DistributedCacheEntryOptions options = new();
            int callCount = 0;
            Foo func()
            {
                callCount++;
                return new Foo() { Value = 2 };
            }

            Foo foo = await cache.GetAndSetAsync<Foo>("foo", func, options, TestContext.Current.CancellationToken);
            Assert.Equal(2, foo.Value);

            Foo foo2 = await cache.GetAndSetAsync<Foo>("foo", func, options, TestContext.Current.CancellationToken);
            Assert.Equivalent(foo, foo2);
            Assert.Equal(1, callCount);
        }

        internal class MemoryDistributedCache : IDistributedCache
        {
            private readonly Dictionary<string, byte[]> _cache = new();

            public byte[] Get(string key)
                => _cache.TryGetValue(key, out byte[] value) ? value : null;

            public Task<byte[]> GetAsync(string key, CancellationToken token = default)
                => Task.FromResult(Get(key));

            public void Refresh(string key)
            {
                throw new NotImplementedException();
            }

            public Task RefreshAsync(string key, CancellationToken token = default)
            {
                throw new NotImplementedException();
            }

            public void Remove(string key)
            {
                throw new NotImplementedException();
            }

            public Task RemoveAsync(string key, CancellationToken token = default)
            {
                throw new NotImplementedException();
            }

            public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
                => _cache.TryAdd(key, value);

            public Task SetAsync(string key,
                byte[] value,
                DistributedCacheEntryOptions options,
                CancellationToken token = default)
            {
                Set(key, value, options);

                return Task.CompletedTask;
            }
        }
    }
}
