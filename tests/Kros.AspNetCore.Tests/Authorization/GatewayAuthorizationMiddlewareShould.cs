using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Kros.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization
{
    public class GatewayAuthorizationMiddlewareShould
    {
        [Fact]
        public async void AddJwtTokenIntoHeader()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK);

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()));

            context.Request.Headers[HeaderNames.Authorization]
                .Should()
                .HaveCount(1)
                .And
                .Contain("Bearer MyJwtToken");
        }

        [Fact]
        public async void AddJwtTokenIntoHeaderForHash()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK);

            var context = new DefaultHttpContext();
            context.Request.Query = new QueryCollection(QueryHelpers.ParseQuery("?hash=asdf"));
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()));

            context.Request.Headers[HeaderNames.Authorization]
                .Should()
                .HaveCount(1)
                .And
                .Contain("Bearer MyJwtToken");
        }

        [Fact]
        public async void UseCachedJwtToken()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK);
            string accessToken = "access_token";

            var context = new DefaultHttpContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            cache.Set(HashCode.Combine(accessToken, context.Request.Path), "AAAAAA");

            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            await middleware.Invoke(context, httpClientFactoryMock, cache);

            context.Request.Headers[HeaderNames.Authorization]
                .Should()
                .HaveCount(1)
                .And
                .Contain("Bearer AAAAAA");
        }

        [Fact]
        public async void UseCachedJwtTokenForHash()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK);

            var context = new DefaultHttpContext();
            context.Request.Query = new QueryCollection(QueryHelpers.ParseQuery("?hash=asdf"));

            var cache = new MemoryCache(new MemoryCacheOptions());
            cache.Set(HashCode.Combine(context.Request.Query["hash"].ToString(), context.Request.Path), "BBQ");

            await middleware.Invoke(context, httpClientFactoryMock, cache);

            context.Request.Headers[HeaderNames.Authorization]
                .Should()
                .HaveCount(1)
                .And
                .Contain("Bearer BBQ");
        }

        [Fact]
        public async void CachesJwtToken()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK, TimeSpan.FromSeconds(5));
            string accessToken = "access_token";

            var context = new DefaultHttpContext();
            var cache = new MemoryCache(new MemoryCacheOptions());

            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            await middleware.Invoke(context, httpClientFactoryMock, cache);

            cache.Get(HashCode.Combine(accessToken, context.Request.Path))
                .Should()
                .Be("MyJwtToken");
        }

        [Fact]
        public async void CachesJwtTokenForHash()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK, TimeSpan.FromSeconds(5));

            var context = new DefaultHttpContext();
            context.Request.Query = new QueryCollection(QueryHelpers.ParseQuery("?hash=asdf"));
            var cache = new MemoryCache(new MemoryCacheOptions());

            await middleware.Invoke(context, httpClientFactoryMock, cache);

            cache.Get(HashCode.Combine(context.Request.Query["hash"].ToString(), context.Request.Path))
                .Should()
                .Be("MyJwtToken");
        }

        [Fact]
        public async void JwtTokenWithoutCaching()
        {
            var ignoredPath = new List<string>();
            ignoredPath.Add("/IgnoredPath");
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK, TimeSpan.FromSeconds(5), ignoredPath);
            string accessToken = "access_token";

            var context = new DefaultHttpContext();
            var cache = new MemoryCache(new MemoryCacheOptions());

            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            context.Request.Path = "/ignoredPath/";
            context.Request.Method = HttpMethod.Get.ToString();
            await middleware.Invoke(context, httpClientFactoryMock, cache);

            var aaa = cache.Get(HashCode.Combine(accessToken, context.Request.Path));
            cache.Get(HashCode.Combine(accessToken, context.Request.Path))
                .Should()
                .BeNull();
        }

        [Fact]
        public async void DoNotAddJwtTokenIntoHeaderWhenAuthorizationHeaderAndHashIsMissing()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK);

            var context = new DefaultHttpContext();
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()));

            context.Request.Headers[HeaderNames.Authorization]
                .Should().BeEmpty();
        }

        [Theory()]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async void ThrowUnauthorizedAccessExceptionWhenIsNotauthorized(HttpStatusCode statusCode)
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(statusCode);

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(()
                => middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions())));
        }

        [Theory()]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async void ThrowUnauthorizedAccessExceptionWhenIsNotAuthorizedForHash(HttpStatusCode statusCode)
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(statusCode);

            var context = new DefaultHttpContext();
            context.Request.Query = new QueryCollection(QueryHelpers.ParseQuery("?hash=asdf"));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(()
                => middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions())));
        }

        [Fact]
        public async void ThrowResourceIsForbiddenExceptionWhenIsNotForbidden()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.Forbidden);

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");

            await Assert.ThrowsAsync<ResourceIsForbiddenException>(()
                => middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions())));
        }

        [Fact]
        public async void ThrowNotFoundExceptionWhenResourceIsNotFound()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.NotFound);

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");

            await Assert.ThrowsAsync<NotFoundException>(()
                => middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions())));
        }

        [Fact]
        public async void ThrowBadRequestExceptionWhenRequestIsBad()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.BadRequest);

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");

            await Assert.ThrowsAsync<BadRequestException>(()
                => middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions())));
        }

        private static (IHttpClientFactory, GatewayAuthorizationMiddleware) CreateMiddleware(HttpStatusCode statusCode)
            => CreateMiddleware(statusCode, TimeSpan.Zero, new List<string>());

        private static (IHttpClientFactory, GatewayAuthorizationMiddleware) CreateMiddleware(
           HttpStatusCode statusCode,
           TimeSpan offset)
            => CreateMiddleware(statusCode, offset, new List<string>());

        private static (IHttpClientFactory, GatewayAuthorizationMiddleware) CreateMiddleware(
            HttpStatusCode statusCode,
            TimeSpan offset,
            List<string> ignoredPathForCache)
        {
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = statusCode,
                Content = new StringContent("MyJwtToken")
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);

            httpClientFactory.CreateClient("JwtAuthorizationClientName").Returns(fakeHttpClient);

            var gatewayJwtAuthorizationOptions = new GatewayJwtAuthorizationOptions()
            {
                AuthorizationUrl = "http://authorizationservice.com",
                HashAuthorizationUrl = "http://hashauthorizationservice.com",
                HashParameterName = "hash",
                CacheSlidingExpirationOffset = offset
            };
            gatewayJwtAuthorizationOptions.IgnoredPathForCache.AddRange(ignoredPathForCache);
            var middleware = new GatewayAuthorizationMiddleware((c) => Task.CompletedTask, gatewayJwtAuthorizationOptions);

            return (httpClientFactory, middleware);
        }
    }
}
