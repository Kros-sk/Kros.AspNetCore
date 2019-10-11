using FluentAssertions;
using Kros.AspNetCore.Net;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Net.Http;
using Xunit;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class HttpClientFactoryExtensionsShould
    {
        [Fact]
        public void CopyAuthorizationHeader()
        {
            var httpClient = new HttpClient();
            IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(httpClient);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer sfasdfasdf414weqr";
            IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            httpContextAccessor.HttpContext.Returns(httpContext);

            using (HttpClient clientAuth = httpClientFactory.CreateClientWithAuthorization(httpContextAccessor))
            {
                clientAuth.DefaultRequestHeaders.Authorization.Should().Equals(httpContext.Request.Headers["Authorization"]);
            }
        }
    }
}
