using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization
{
    public class JwtBearerClaimsMiddlewareShould
    {
        [Fact]
        public async Task NotFailOnNoAuthHeader()
        {
            HttpContext context = CreateHttpContext();
            JwtBearerClaimsMiddleware middleware = CreateMiddleware();

            await middleware.Invoke(context);
        }

        [Fact]
        public async Task NotFailOnNonBearerToken()
        {
            HttpContext context = CreateHttpContext("NonBearerToken");
            JwtBearerClaimsMiddleware middleware = CreateMiddleware();

            await middleware.Invoke(context);
        }

        [Fact]
        public async Task NotFailOnNonJwtBearerToekn()
        {
            HttpContext context = CreateHttpContext("Bearer NonJwtToken");
            JwtBearerClaimsMiddleware middleware = CreateMiddleware();

            await middleware.Invoke(context);
        }

        [Fact]
        public async Task AddClaimsToHttpContextUserIdentity()
        {
            string token = CreateJwtToken(("test", "testvalue"), ("test2", "testvalue2"));
            HttpContext context = CreateHttpContext(token);
            JwtBearerClaimsMiddleware middleware = CreateMiddleware();

            await middleware.Invoke(context);

            context.User.HasClaim("test", "testvalue").Should().BeTrue();
            context.User.HasClaim("test2", "testvalue2").Should().BeTrue();
        }

        private static string CreateJwtToken(params (string Key, string Value)[] claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.Select(c => new Claim(c.Key, c.Value)).ToArray()),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return $"Bearer {tokenHandler.WriteToken(token)}";
        }

        private static JwtBearerClaimsMiddleware CreateMiddleware()
            => new((c) => Task.CompletedTask, new JwtSecurityTokenHandler());

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
