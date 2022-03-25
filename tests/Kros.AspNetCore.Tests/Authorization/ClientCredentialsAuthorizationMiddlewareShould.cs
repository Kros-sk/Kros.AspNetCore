using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Kros.AspNetCore.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization
{
    public class ClientCredentialsAuthorizationMiddlewareShould
    {
        [Fact]
        public async Task ThrowUnauthorizedAccessExceptionOnNonAuthHeader()
        {
            HttpContext context = CreateHttpContext();
            ClientCredentialsAuthorizationMiddleware middleware = CreateMiddleware();
            Func<Task> funcInvoke = async () => await middleware.Invoke(context);
            await funcInvoke.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Theory]
        [InlineData("NonBearerToken")]
        [InlineData("Bearer NonJwtToken")]
        public async Task ThrowExceptionOnBadAuthHeader(string authHeader)
        {
            HttpContext context = CreateHttpContext(authHeader);
            ClientCredentialsAuthorizationMiddleware middleware = CreateMiddleware();
            Func<Task> funcInvoke = async () => await middleware.Invoke(context);
            await funcInvoke.Should().ThrowAsync<Exception>();
        }

        private static ClientCredentialsAuthorizationMiddleware CreateMiddleware()
           => new(
               (c) => Task.CompletedTask,
               new ClientCredentialsAuthorizationOptions { AuthorityUrl = "http://any.com", Scope = "123"});

        private static HttpContext CreateHttpContext(string authHeader = null)
        {
            HttpContext context = new DefaultHttpContext();
            if (authHeader != null)
            {
                context.Request.Headers.Add(HeaderNames.Authorization, authHeader);
            }
            return context;
        }
    }
}
