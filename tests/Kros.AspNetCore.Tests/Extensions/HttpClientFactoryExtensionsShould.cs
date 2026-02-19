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
            HttpClient httpClient = new();
            IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(httpClient);

            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers["Authorization"] = "Bearer sfasdfasdf414weqr";
            IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            httpContextAccessor.HttpContext.Returns(httpContext);

            using (HttpClient clientAuth = httpClientFactory.CreateClientWithAuthorization(httpContextAccessor))
            {
                Assert.Equal(httpContext.Request.Headers["Authorization"].ToString(), clientAuth.DefaultRequestHeaders.Authorization.ToString());
            }
        }
    }
}
