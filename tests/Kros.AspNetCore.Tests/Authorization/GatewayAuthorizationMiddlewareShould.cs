﻿using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Kros.AspNetCore.Exceptions;
using Kros.AspNetCore.ServiceDiscovery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.OK);

            DefaultHttpContext context = new();
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
            List<string> forwardedHeaders = new() { "ApplicationType" };

            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware, HttpClient htttpClient)
                = CreateMiddlewareForForwardedHeaders(HttpStatusCode.OK, forwardedHeaders);

            DefaultHttpContext context = new();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            context.Request.Headers.Add("ApplicationType", "25");
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()), CreateProvider());

            htttpClient.DefaultRequestHeaders.GetValues("ApplicationType").Should().Equal("25");
        }

        [Fact]
        public async void NotAddCustomHeaderToHeadersIfItIsNotInOptions()
        {
            List<string> forwardedHeaders = new();

            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware, HttpClient htttpClient)
                = CreateMiddlewareForForwardedHeaders(HttpStatusCode.OK, forwardedHeaders);

            DefaultHttpContext context = new();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            context.Request.Headers.Add("ApplicationType", "25");
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()), CreateProvider());

            htttpClient.DefaultRequestHeaders.TryGetValues("ApplicationType", out IEnumerable<string> _).Should().BeFalse();
        }

        [Fact]
        public async void AddJwtTokenIntoHeaderWithServiceDiscovery()
        {
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(
                HttpStatusCode.OK,
                TimeSpan.Zero,
                new List<string>(),
                new GatewayJwtAuthorizationOptions()
                {
                    Authorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    HashAuthorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" }
                });

            DefaultHttpContext context = new();
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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.OK);

            DefaultHttpContext context = new();
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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(
                HttpStatusCode.OK,
                TimeSpan.Zero,
                new List<string>(),
                new GatewayJwtAuthorizationOptions()
                {
                    Authorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    HashAuthorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    HashParameterName = "hash"
                });

            DefaultHttpContext context = new();
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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.OK);

            DefaultHttpContext context = new();
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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.OK);
            string accessToken = "access_token";

            DefaultHttpContext context = new();
            MemoryCache cache = new(new MemoryCacheOptions());
            cache.Set(HashCode.Combine(accessToken), "AAAAAA");

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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.OK);

            DefaultHttpContext context = new();
            context.Request.Query = new QueryCollection(QueryHelpers.ParseQuery("?hash=asdf"));

            MemoryCache cache = new(new MemoryCacheOptions());
            cache.Set(HashCode.Combine(context.Request.Query["hash"].ToString()), "BBQ");

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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.OK, TimeSpan.FromSeconds(5));
            string accessToken = "access_token";

            DefaultHttpContext context = new();
            MemoryCache cache = new(new MemoryCacheOptions());

            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            await middleware.Invoke(context, httpClientFactoryMock, cache, CreateProvider());

            cache.Get(HashCode.Combine(accessToken))
                .Should()
                .Be(JwtToken);
        }

        [Fact]
        public async void CachesJwtTokenForHash()
        {
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.OK, TimeSpan.FromSeconds(5));

            DefaultHttpContext context = new();
            context.Request.Query = new QueryCollection(QueryHelpers.ParseQuery("?hash=asdf"));
            MemoryCache cache = new(new MemoryCacheOptions());

            await middleware.Invoke(context, httpClientFactoryMock, cache, CreateProvider());

            cache.Get(HashCode.Combine(context.Request.Query["hash"].ToString()))
                .Should()
                .Be(HashJwtToken);
        }

        [Fact]
        public async void JwtTokenWithoutCaching()
        {
            List<string> ignoredPath = new();
            ignoredPath.Add("/IgnoredPath");
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.OK, TimeSpan.FromSeconds(5), ignoredPath);
            string accessToken = "access_token";

            DefaultHttpContext context = new();
            MemoryCache cache = new(new MemoryCacheOptions());

            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");
            context.Request.Path = "/ignoredPath/";
            context.Request.Method = HttpMethod.Get.ToString();
            await middleware.Invoke(context, httpClientFactoryMock, cache, CreateProvider());

            cache.Get(HashCode.Combine(accessToken))
                .Should()
                .BeNull();
        }

        [Fact]
        public async void DoNotAddJwtTokenIntoHeaderWhenAuthorizationHeaderAndHashIsMissing()
        {
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.OK);

            DefaultHttpContext context = new();
            await middleware.Invoke(context, httpClientFactoryMock, new MemoryCache(new MemoryCacheOptions()), CreateProvider());

            context.Request.Headers[HeaderNames.Authorization]
                .Should().BeEmpty();
        }

        [Theory()]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async void ThrowUnauthorizedAccessExceptionWhenIsNotauthorized(HttpStatusCode statusCode)
        {
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(statusCode);

            DefaultHttpContext context = new();
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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(statusCode);

            DefaultHttpContext context = new();
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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.Forbidden);

            DefaultHttpContext context = new();
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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.NotFound);

            DefaultHttpContext context = new();
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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(HttpStatusCode.BadRequest);

            DefaultHttpContext context = new();
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
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(
                HttpStatusCode.OK,
                TimeSpan.Zero,
                new List<string>(),
                new GatewayJwtAuthorizationOptions()
                {
                    Authorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    HashAuthorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    CacheKeyHttpHeaders = new List<string>() { "Connection-Id" }
                });

            DefaultHttpContext context = new();
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
        public async void CacheJwtToken(string connectionId)
        {
            (IHttpClientFactory httpClientFactoryMock, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(
                HttpStatusCode.OK,
                TimeSpan.Zero,
                new List<string>(),
                new GatewayJwtAuthorizationOptions()
                {
                    Authorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    HashAuthorization = new AuthorizationServiceOptions() { ServiceName = "Authorization", PathName = "jwt" },
                    CacheKeyHttpHeaders = new List<string>() { "Connection-Id" }
                });
            const string accessToken = "access_token";
            DefaultHttpContext context = new();
            context.Request.Headers.Add(HeaderNames.Authorization, accessToken);
            context.Request.Headers.Add("Connection-Id", connectionId);
            context.Request.Headers.Add("any-header", connectionId);

            int key = connectionId == null
                ? GatewayAuthorizationMiddleware.GetKey(accessToken)
                : GatewayAuthorizationMiddleware.GetKey(accessToken, connectionId);

            MemoryCache memoryCache = new(new MemoryCacheOptions());
            memoryCache.Set(key, $"{JwtToken}");
            await middleware.Invoke(context, httpClientFactoryMock, memoryCache, CreateProvider());

            context.Request.Headers[HeaderNames.Authorization]
                .Should()
                .HaveCount(1)
                .And
                .Contain($"Bearer {JwtToken}");
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", null)]
        [InlineData("/companies/123456", "/companies/(?<companyId>\\d+)", "123456")]
        [InlineData("/companies/123456789/other-segment", "/companies/(?<companyId>\\d+)/other-segment", "123456789")]
        [InlineData("/companies/123456", null, null)]
        [InlineData("", "/companies/(?<companyId>\\d+)", null)]
        public void CacheUrlPath(string requestPath, string urlPathRegexPattern, string expectedResult)
        {
            (_, GatewayAuthorizationMiddleware middleware) = CreateMiddleware(
                HttpStatusCode.OK,
                TimeSpan.Zero,
                new List<string>(),
                new GatewayJwtAuthorizationOptions()
                {
                    CacheKeyUrlPathRegexPattern = urlPathRegexPattern
                });
            DefaultHttpContext context = new();
            context.Request.Path = requestPath;

            string urlPathForCache = middleware.GetUrlPathForCacheKey(context);

            urlPathForCache.Should().Be(expectedResult);
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

            IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();

            FakeHttpMessageHandler fakeHttpMessageHandler = CreateFakeHttpMessageHandler(statusCode);
            HttpClient fakeHttpClient = new(fakeHttpMessageHandler);
            httpClientFactory.CreateClient("JwtAuthorizationClientName").Returns(fakeHttpClient);

            GatewayAuthorizationMiddleware middleware = new((c) => Task.CompletedTask, options);

            return (httpClientFactory, middleware);
        }

        private static (IHttpClientFactory, GatewayAuthorizationMiddleware, HttpClient) CreateMiddlewareForForwardedHeaders(
            HttpStatusCode statusCode,
            List<string> forwardedHeaders)
        {
            GatewayJwtAuthorizationOptions options = new()
            {
                AuthorizationUrl = AuthorizationUrl,
                HashAuthorizationUrl = HashAuthorizationUrl,
                HashParameterName = "hash",
                CacheSlidingExpirationOffset = TimeSpan.Zero
            };
            options.ForwardedHeaders.AddRange(forwardedHeaders);

            IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();

            FakeHttpMessageHandler fakeHttpMessageHandler = CreateFakeHttpMessageHandler(statusCode);
            HttpClient fakeHttpClient = new(fakeHttpMessageHandler);
            httpClientFactory.CreateClient("JwtAuthorizationClientName").Returns(fakeHttpClient);

            GatewayAuthorizationMiddleware middleware = new((c) => Task.CompletedTask, options);

            return (httpClientFactory, middleware, fakeHttpClient);
        }

        private static FakeHttpMessageHandler CreateFakeHttpMessageHandler(HttpStatusCode statusCode)
        {
            List<KeyValuePair<FakeHttpMessageHandler.HttpRequestFilter, HttpResponseMessage>> responses = new();
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

        private static IServiceDiscoveryProvider CreateProvider(string url = null)
        {
            IServiceDiscoveryProvider provider = Substitute.For<IServiceDiscoveryProvider>();
            provider.GetPath(Arg.Any<string>(), Arg.Any<string>()).Returns(new Uri(url ?? AuthorizationUrl));

            return provider;
        }
    }
}
