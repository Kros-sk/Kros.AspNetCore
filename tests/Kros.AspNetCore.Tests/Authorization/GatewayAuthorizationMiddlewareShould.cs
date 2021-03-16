using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Kros.AspNetCore.Exceptions;
using Kros.AspNetCore.ServiceDiscovery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
        private const string AuthorizationUrl = "http://authorizationservice.com";
        private const string HashAuthorizationUrl = "http://hashauthorizationservice.com";
        private const string JwtToken = "MyJwtToken";
        private const string HashJwtToken = "HashJwtToken";

        [Fact]
        public async void AddJwtTokenIntoHeader()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK);

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()), CreateProvider());

            context.Request.Headers[HeaderNames.Authorization]
                .Should()
                .HaveCount(1)
                .And
                .Contain("Bearer MyJwtToken");
        }

        [Fact]
        public async void AddCustomHeaderToHeadersIfItIsInOptions()
        {
            List<string> forwardedHeaders = new List<string>() { "ApplicationType" };

            (var httpClientFactoryMock, var middleware, var htttpClient) = CreateMiddlewareForForwardedHeaders(
                HttpStatusCode.OK,
                forwardedHeaders
                );

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            context.Request.Headers.Add("ApplicationType", "25");
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()), CreateProvider());

            htttpClient.DefaultRequestHeaders.GetValues("ApplicationType").Should().Equal("25");
        }

        [Fact]
        public async void NotAddCustomHeaderToHeadersIfItIsNotInOptions()
        {
            List<string> forwardedHeaders = new List<string>();

            (var httpClientFactoryMock, var middleware, var htttpClient) = CreateMiddlewareForForwardedHeaders(
                HttpStatusCode.OK,
                forwardedHeaders
                );

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            context.Request.Headers.Add("ApplicationType", "25");
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()), CreateProvider());

            htttpClient.DefaultRequestHeaders.TryGetValues("ApplicationType", out IEnumerable<string> values).Should().BeFalse();
        }

        [Fact]
        public async void AddJwtTokenIntoHeaderWithServiceDiscovery()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(
                HttpStatusCode.OK,
                TimeSpan.Zero,
                new List<string>(),
                new GatewayJwtAuthorizationOptions()
                {
                    Authorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    HashAuthorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" }
                });

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()), CreateProvider());

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
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()), CreateProvider());

            context.Request.Headers[HeaderNames.Authorization]
                .Should()
                .HaveCount(1)
                .And
                .Contain("Bearer HashJwtToken");
        }

        [Fact]
        public async void AddJwtTokenIntoHeaderForHashWithServiceDiscovery()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(
                HttpStatusCode.OK,
                TimeSpan.Zero,
                new List<string>(),
                new GatewayJwtAuthorizationOptions()
                {
                    Authorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    HashAuthorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    HashParameterName = "hash"
                });

            var context = new DefaultHttpContext();
            context.Request.Query = new QueryCollection(QueryHelpers.ParseQuery("?hash=asdf"));
            await middleware.Invoke(
                context,
                httpClientFactoryMock,
                new MemoryCache(new MemoryCacheOptions()),
                CreateProvider(HashAuthorizationUrl));

            context.Request.Headers[HeaderNames.Authorization]
                .Should()
                .HaveCount(1)
                .And
                .Contain("Bearer HashJwtToken");
        }

        [Fact]
        public async void AddJwtTokenIntoHeader_WithAccessTokenPriorityOverHash()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK);

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            context.Request.Query = new QueryCollection(QueryHelpers.ParseQuery("?hash=asdf"));
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()), CreateProvider());

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
            await middleware.Invoke(context, httpClientFactoryMock, cache, CreateProvider());

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

            await middleware.Invoke(context, httpClientFactoryMock, cache, CreateProvider());

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
            await middleware.Invoke(context, httpClientFactoryMock, cache, CreateProvider());

            cache.Get(HashCode.Combine(accessToken, context.Request.Path))
                .Should()
                .Be(JwtToken);
        }

        [Fact]
        public async void CachesJwtTokenForHash()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK, TimeSpan.FromSeconds(5));

            var context = new DefaultHttpContext();
            context.Request.Query = new QueryCollection(QueryHelpers.ParseQuery("?hash=asdf"));
            var cache = new MemoryCache(new MemoryCacheOptions());

            await middleware.Invoke(context, httpClientFactoryMock, cache, CreateProvider());

            cache.Get(HashCode.Combine(context.Request.Query["hash"].ToString(), context.Request.Path))
                .Should()
                .Be(HashJwtToken);
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
            await middleware.Invoke(context, httpClientFactoryMock, cache, CreateProvider());

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
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()), CreateProvider());

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
                => middleware.Invoke(
                    context,
                    httpClientFactoryMock,
                    new MemoryCache(new MemoryCacheOptions()),
                    CreateProvider()));
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
                => middleware.Invoke(
                    context,
                    httpClientFactoryMock,
                    new MemoryCache(new MemoryCacheOptions()),
                    CreateProvider()));
        }

        [Fact]
        public async void ThrowResourceIsForbiddenExceptionWhenIsNotForbidden()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.Forbidden);

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");

            await Assert.ThrowsAsync<ResourceIsForbiddenException>(()
                => middleware.Invoke(
                    context,
                    httpClientFactoryMock,
                    new MemoryCache(new MemoryCacheOptions()),
                    CreateProvider()));
        }

        [Fact]
        public async void ThrowNotFoundExceptionWhenResourceIsNotFound()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.NotFound);

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");

            await Assert.ThrowsAsync<NotFoundException>(()
                => middleware.Invoke(
                    context,
                    httpClientFactoryMock,
                    new MemoryCache(new MemoryCacheOptions()),
                    CreateProvider()));
        }

        [Fact]
        public async void ThrowBadRequestExceptionWhenRequestIsBad()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.BadRequest);

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");

            await Assert.ThrowsAsync<BadRequestException>(()
                => middleware.Invoke(
                    context,
                    httpClientFactoryMock,
                    new MemoryCache(new MemoryCacheOptions()),
                    CreateProvider()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("connection_id")]
        public async void JwtTokenDoesNotContainConnectionId(string connectionId)
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(
                HttpStatusCode.OK,
                TimeSpan.Zero,
                new List<string>(),
                new GatewayJwtAuthorizationOptions()
                {
                    Authorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    HashAuthorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    CacheKeyHttpHeaders = new List<string>() { "Connection-Id" }
                });

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            context.Request.Headers.Add("Connection-Id", connectionId);
            context.Request.Headers.Add("any-header", connectionId);
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()), CreateProvider());

            context.Request.Headers[HeaderNames.Authorization]
                .Should()
                .HaveCount(1)
                .And
                .Contain($"Bearer {JwtToken}");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("connection_id")]
        public async void CacheJwtTokenWithConnectionId(string connectionId)
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(
                HttpStatusCode.OK,
                TimeSpan.Zero,
                new List<string>(),
                new GatewayJwtAuthorizationOptions()
                {
                    Authorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    HashAuthorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    CacheKeyHttpHeaders = new List<string>() { "Connection-Id"}
                });
            const string accessToken = "access_token";
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, accessToken);
            context.Request.Headers.Add("Connection-Id", connectionId);
            context.Request.Headers.Add("any-header", connectionId);

            int key = connectionId == null
                ? GatewayAuthorizationMiddleware.GetKey(context, accessToken)
                : GatewayAuthorizationMiddleware.GetKey(context, accessToken, connectionId);

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set(key, $"{JwtToken}");
            await middleware.Invoke(context, httpClientFactoryMock, memoryCache, CreateProvider());

            context.Request.Headers[HeaderNames.Authorization]
                .Should()
                .HaveCount(1)
                .And
                .Contain($"Bearer {JwtToken}");
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
            List<string> ignoredPathForCache,
            GatewayJwtAuthorizationOptions options = null)
        {
            if (options is null)
            {
                options = new GatewayJwtAuthorizationOptions()
                {
                    AuthorizationUrl = AuthorizationUrl,
                    HashAuthorizationUrl = HashAuthorizationUrl,
                    HashParameterName = "hash",
                    CacheSlidingExpirationOffset = offset
                };
            }
            options.IgnoredPathForCache.AddRange(ignoredPathForCache);

            var httpClientFactory = Substitute.For<IHttpClientFactory>();

            FakeHttpMessageHandler fakeHttpMessageHandler = CreateFakeHttpMessageHandler(statusCode);
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactory.CreateClient("JwtAuthorizationClientName").Returns(fakeHttpClient);

            var middleware = new GatewayAuthorizationMiddleware((c) => Task.CompletedTask, options);

            return (httpClientFactory, middleware);
        }

        private static (IHttpClientFactory, GatewayAuthorizationMiddleware, HttpClient) CreateMiddlewareForForwardedHeaders(
            HttpStatusCode statusCode,
            List<string> forwardedHeaders)
        {
            GatewayJwtAuthorizationOptions options = new GatewayJwtAuthorizationOptions()
            {
                AuthorizationUrl = AuthorizationUrl,
                HashAuthorizationUrl = HashAuthorizationUrl,
                HashParameterName = "hash",
                CacheSlidingExpirationOffset = TimeSpan.Zero
            };
            options.ForwardedHeaders.AddRange(forwardedHeaders);

            var httpClientFactory = Substitute.For<IHttpClientFactory>();

            FakeHttpMessageHandler fakeHttpMessageHandler = CreateFakeHttpMessageHandler(statusCode);
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactory.CreateClient("JwtAuthorizationClientName").Returns(fakeHttpClient);

            var middleware = new GatewayAuthorizationMiddleware((c) => Task.CompletedTask, options);

            return (httpClientFactory, middleware, fakeHttpClient);
        }

        private static FakeHttpMessageHandler CreateFakeHttpMessageHandler(HttpStatusCode statusCode)
        {
            var responses = new List<KeyValuePair<FakeHttpMessageHandler.HttpRequestFilter, HttpResponseMessage>>();
            responses.Add(new KeyValuePair<FakeHttpMessageHandler.HttpRequestFilter, HttpResponseMessage>(
                (req) => req.RequestUri.AbsoluteUri.Contains(AuthorizationUrl),
                new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                    Content = new StringContent(JwtToken)
                }));
            responses.Add(new KeyValuePair<FakeHttpMessageHandler.HttpRequestFilter, HttpResponseMessage>(
                (req) => req.RequestUri.AbsoluteUri.Contains(HashAuthorizationUrl),
                new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                    Content = new StringContent(HashJwtToken)
                }));

            return new FakeHttpMessageHandler(responses);
        }

        private IServiceDiscoveryProvider CreateProvider(string url = null)
        {
            IServiceDiscoveryProvider provider = Substitute.For<IServiceDiscoveryProvider>();
            provider.GetPath(Arg.Any<string>(), Arg.Any<string>()).Returns(new Uri(url ?? AuthorizationUrl));

            return provider;
        }
    }
}
