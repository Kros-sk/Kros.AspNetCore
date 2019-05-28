using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using System;
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
            await middleware.Invoke(context, httpClientFactoryMock);

            context.Request.Headers[HeaderNames.Authorization]
                .Should()
                .HaveCount(1)
                .And
                .Contain("Bearer MyJwtToken");
        }

        [Fact]
        public async void DoNotAddJwtTokenIntoHeaderWhenAuthorizationHeaderIsMissing()
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(HttpStatusCode.OK);

            var context = new DefaultHttpContext();
            await middleware.Invoke(context, httpClientFactoryMock);

            context.Request.Headers[HeaderNames.Authorization]
                .Should().BeEmpty();
        }

        [Theory()]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.BadRequest)]
        public void ThrowUnauthorizedAccessExceptionWhenIsNotauthorized(HttpStatusCode statusCode)
        {
            (var httpClientFactoryMock, var middleware) = CreateMiddleware(statusCode);

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "access_token");

            Assert.ThrowsAsync<UnauthorizedAccessException>(async ()
                => await middleware.Invoke(context, httpClientFactoryMock));
        }

        private static (IHttpClientFactory, GatewayAuthorizationMiddleware) CreateMiddleware(HttpStatusCode statusCode)
        {
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = statusCode,
                Content = new StringContent("MyJwtToken")
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);

            httpClientFactory.CreateClient("JwtAuthorizationClientName").Returns(fakeHttpClient);

            var middleware = new GatewayAuthorizationMiddleware((c) =>
            {
                return Task.CompletedTask;
            }, new GatewayJwtAuthorizationOptions() { AuthorizationUrl = "http://authorizationservice.com" });

            return (httpClientFactory, middleware);
        }
    }
}
