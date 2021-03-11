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
        public void ThrowUnauthorizedAccessExceptionOnNonAuthHeader()
        {
            HttpContext context = CreateHttpContext();
            ClientCredentialsAuthorizationMiddleware middleware = CreateMiddleware();
            Func<Task> funcInvoke = async () => await middleware.Invoke(context);
            funcInvoke.Should().Throw<UnauthorizedAccessException>();
        }

        [Theory]
        [InlineData("NonBearerToken")]
        [InlineData("Bearer NonJwtToken")]
        public void ThrowExceptionOnBadAuthHeader(string authHeader)
        {
            HttpContext context = CreateHttpContext(authHeader);
            ClientCredentialsAuthorizationMiddleware middleware = CreateMiddleware();
            Func<Task> funcInvoke = async () => await middleware.Invoke(context);
            funcInvoke.Should().Throw<Exception>();
        }

        private ClientCredentialsAuthorizationMiddleware CreateMiddleware()
           => new ClientCredentialsAuthorizationMiddleware(
               (c) => Task.CompletedTask,
               new ClientCredentialsAuthorizationOptions { AuthorityUrl = "http://any.com", Scope = "123"});

        private HttpContext CreateHttpContext(string authHeader = null)
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
