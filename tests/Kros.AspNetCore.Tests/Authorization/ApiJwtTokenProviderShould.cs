using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Kros.AspNetCore.Exceptions;
using Kros.AspNetCore.ServiceDiscovery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization
{
    public class ApiJwtTokenProviderShould
    {
        private const string AuthorizationUrl = "http://authorizationservice.com";
        private const string HashAuthorizationUrl = "http://hashauthorizationservice.com";
        private const string JwtToken = "MyJwtToken";
        private const string HashJwtToken = "HashJwtToken";

        [Fact]
        public async Task GetJwtTokenAsync_ReturnsJwtToken_WhenAuthorizationServiceRespondsOk()
        {
            ApiJwtTokenProvider provider = CreateProvider(HttpStatusCode.OK);
            StringValues token = "access_token";

            string result = await provider.GetJwtTokenAsync(token);

            result.Should().Be(JwtToken);
        }

        [Fact]
        public async Task GetJwtTokenForHashAsync_ReturnsJwtToken_WhenAuthorizationServiceRespondsOk()
        {
            ApiJwtTokenProvider provider = CreateProvider(HttpStatusCode.OK, hashUrl: HashAuthorizationUrl);
            StringValues hashValue = "hash_value";

            string result = await provider.GetJwtTokenForHashAsync(hashValue);

            result.Should().Be(HashJwtToken);
        }

        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetJwtTokenAsync_ThrowsUnauthorizedAccessException_WhenNotAuthorized(
            HttpStatusCode statusCode)
        {
            ApiJwtTokenProvider provider = CreateProvider(statusCode);
            StringValues token = "access_token";

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetJwtTokenAsync(token));
        }

        [Fact]
        public async Task GetJwtTokenAsync_ThrowsResourceIsForbiddenException_WhenForbidden()
        {
            ApiJwtTokenProvider provider = CreateProvider(HttpStatusCode.Forbidden);
            StringValues token = "access_token";

            await Assert.ThrowsAsync<ResourceIsForbiddenException>(() => provider.GetJwtTokenAsync(token));
        }

        [Fact]
        public async Task GetJwtTokenAsync_ThrowsNotFoundException_WhenNotFound()
        {
            ApiJwtTokenProvider provider = CreateProvider(HttpStatusCode.NotFound);
            StringValues token = "access_token";

            await Assert.ThrowsAsync<NotFoundException>(() => provider.GetJwtTokenAsync(token));
        }

        [Fact]
        public async Task GetJwtTokenAsync_ThrowsBadRequestException_WhenBadRequest()
        {
            ApiJwtTokenProvider provider = CreateProvider(HttpStatusCode.BadRequest);
            StringValues token = "access_token";

            await Assert.ThrowsAsync<BadRequestException>(() => provider.GetJwtTokenAsync(token));
        }

        [Fact]
        public async Task GetJwtTokenAsync_AddsForwardedHeaders_WhenConfigured()
        {
            GatewayJwtAuthorizationOptions options = new()
            {
                AuthorizationUrl = AuthorizationUrl
            };
            options.ForwardedHeaders.Add("ApplicationType");

            IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();
            IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            IServiceDiscoveryProvider serviceDiscoveryProvider = Substitute.For<IServiceDiscoveryProvider>();

            DefaultHttpContext context = new();
            context.Request.Headers["ApplicationType"] = "25";
            httpContextAccessor.HttpContext.Returns(context);

            FakeHttpMessageHandler fakeHandler = new(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JwtToken)
            });
            HttpClient httpClient = new(fakeHandler);
            httpClientFactory.CreateClient(ApiJwtTokenProvider.AuthorizationHttpClientName).Returns(httpClient);

            ApiJwtTokenProvider provider = new(
                httpClientFactory,
                httpContextAccessor,
                Microsoft.Extensions.Options.Options.Create(options),
                serviceDiscoveryProvider);

            string result = await provider.GetJwtTokenAsync("access_token");

            result.Should().Be(JwtToken);
            httpClient.DefaultRequestHeaders.Contains("ApplicationType").Should().BeTrue();
            httpClient.DefaultRequestHeaders.GetValues("ApplicationType").Should().Contain("25");
        }

        private static ApiJwtTokenProvider CreateProvider(
            HttpStatusCode statusCode,
            string authUrl = AuthorizationUrl,
            string hashUrl = HashAuthorizationUrl)
        {
            GatewayJwtAuthorizationOptions options = new()
            {
                AuthorizationUrl = authUrl,
                HashAuthorizationUrl = hashUrl,
                HashParameterName = "hash"
            };

            IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();
            IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            IServiceDiscoveryProvider serviceDiscoveryProvider = Substitute.For<IServiceDiscoveryProvider>();

            DefaultHttpContext context = new();
            httpContextAccessor.HttpContext.Returns(context);

            List<KeyValuePair<FakeHttpMessageHandler.HttpRequestFilter, HttpResponseMessage>> responses =
            [
                new KeyValuePair<FakeHttpMessageHandler.HttpRequestFilter, HttpResponseMessage>(
                    req => req.RequestUri.AbsoluteUri.Contains(authUrl),
                    new HttpResponseMessage
                    {
                        StatusCode = statusCode,
                        Content = new StringContent(JwtToken)
                    }),
                new KeyValuePair<FakeHttpMessageHandler.HttpRequestFilter, HttpResponseMessage>(
                    req => req.RequestUri.AbsoluteUri.Contains(hashUrl),
                    new HttpResponseMessage
                    {
                        StatusCode = statusCode,
                        Content = new StringContent(HashJwtToken)
                    })
            ];

            FakeHttpMessageHandler fakeHandler = new(responses);
            HttpClient httpClient = new(fakeHandler);
            httpClientFactory.CreateClient(ApiJwtTokenProvider.AuthorizationHttpClientName).Returns(httpClient);

            return new ApiJwtTokenProvider(
                httpClientFactory,
                httpContextAccessor,
                Microsoft.Extensions.Options.Options.Create(options),
                serviceDiscoveryProvider);
        }
    }
}
