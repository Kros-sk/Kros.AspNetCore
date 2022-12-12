using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization
{
    public class JwtAuthorizationHelperShould
    {
        [Fact]
        public void CreateJwtTokensFromClaims()
        {
            IEnumerable<Claim> claims = CreateClaims(11, "bob@bob.com");
            string jwt = JwtAuthorizationHelper
                .CreateJwtTokenFromClaims(claims, "top_secreat_password", new DateTime(2200, 1, 1));

            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken tokenS = handler.ReadToken(jwt) as JwtSecurityToken;

            tokenS.Claims.First(claim => claim.Type == UserClaimTypes.Email).Value
                .Should().Be("bob@bob.com");
            tokenS.Claims.First(claim => claim.Type == UserClaimTypes.UserId).Value
                .Should().Be("11");
        }

        [Fact]
        public void CreateDifferentJwtTokensForDifferentClaims()
        {
            IEnumerable<Claim> claims1 = CreateClaims(11, "bob@bob.com");
            string jwt1 = JwtAuthorizationHelper
                .CreateJwtTokenFromClaims(claims1, "top_secreat_password", new DateTime(2200, 1, 1));

            IEnumerable<Claim> claims2 = CreateClaims(22, "alice@gmail.com");
            string jwt2 = JwtAuthorizationHelper
                .CreateJwtTokenFromClaims(claims2, "top_secreat_password", new DateTime(2200, 1, 1));

            jwt1.Should().NotBe(jwt2);
        }

        [Fact]
        public void TryGetTokenValueReturnsNullToken()
        {
            IHeaderDictionary headers = new HeaderDictionary();
            bool contains = JwtAuthorizationHelper.TryGetTokenValue(headers, out string token);

            contains.Should().BeFalse();
            token.Should().BeNull();
        }

        [Theory]
        [InlineData("", false, true, "")]
        [InlineData("123", false, true, "123")]
        [InlineData("Bearer 123", false, true, "Bearer 123")]
        [InlineData("", true, true, "")]
        [InlineData("123", true, true, "123")]
        [InlineData("Bearer 123", true, true, "123")]
        public void TryGetTokenValueReturnsCorrectToken(
            string token,
            bool removePrefix,
            bool expectedContains,
            string expectedToken)
        {
            IHeaderDictionary headers = new HeaderDictionary();
            headers.Add(HeaderNames.Authorization, token);
            bool contains = JwtAuthorizationHelper.TryGetTokenValue(headers, out token, removePrefix);
            contains.Should().Be(expectedContains);
            token.Should().Be(expectedToken);
        }

        private static IEnumerable<Claim> CreateClaims(int userId, string userEmail)
            => new Claim[]
            {
                 new Claim(UserClaimTypes.UserId, userId.ToString()),
                 new Claim(UserClaimTypes.Email, userEmail)
            };
    }
}
