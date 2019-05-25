using FluentAssertions;
using Kros.AspNetCore.Authorization;
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
            var claims = CreateClaims(11, "bob@bob.com");
            var jwt = JwtAuthorizationHelper
                .CreateJwtTokenFromClaims(claims, "top_secreat_password", new DateTime(2200, 1, 1));

            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(jwt) as JwtSecurityToken;

            tokenS.Claims.First(claim => claim.Type == UserClaimTypes.Email).Value
                .Should().Be("bob@bob.com");
            tokenS.Claims.First(claim => claim.Type == UserClaimTypes.UserId).Value
                .Should().Be("11");
        }

        [Fact]
        public void CreateDifferentJwtTokensForDifferentClaims()
        {
            var claims1 = CreateClaims(11, "bob@bob.com");
            var jwt1 = JwtAuthorizationHelper
                .CreateJwtTokenFromClaims(claims1, "top_secreat_password", new DateTime(2200, 1, 1));

            var claims2 = CreateClaims(22, "alice@gmail.com");
            var jwt2 = JwtAuthorizationHelper
                .CreateJwtTokenFromClaims(claims2, "top_secreat_password", new DateTime(2200, 1, 1));

            jwt1.Should().NotBe(jwt2);
        }

        private IEnumerable<Claim> CreateClaims(int userId, string userEmail)
            => new Claim[]
            {
                 new Claim(UserClaimTypes.UserId, userId.ToString()),
                 new Claim(UserClaimTypes.Email, userEmail)
            };
    }
}
