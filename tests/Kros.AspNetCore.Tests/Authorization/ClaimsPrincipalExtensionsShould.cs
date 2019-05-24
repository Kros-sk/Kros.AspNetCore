using FluentAssertions;
using Kros.AspNetCore.Authorization;
using System.Security.Claims;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization
{
    public class ClaimsPrincipalExtensionsShould
    {
        [Fact]
        public void GetUserIdWhenExist()
        {
            ClaimsPrincipal user = CreateUser();

            user.GetUserId()
                .Should().Be(22);
        }

        [Fact]
        public void GetEmailWhenExist()
        {
            ClaimsPrincipal user = CreateUser();

            user.GetUserEmail()
                .Should().Be("bob@gmail.com");
        }

        [Fact]
        public void GetDefaultUserIdWhenDoNotExist()
        {
            ClaimsPrincipal user = new ClaimsPrincipal();

            user.GetUserId()
                .Should().Be(0);
        }

        [Fact]
        public void GetDefaultUserEmailWhenDoNotExist()
        {
            ClaimsPrincipal user = new ClaimsPrincipal();

            user.GetUserEmail()
                .Should().BeEmpty();
        }

        private static ClaimsPrincipal CreateUser()
            => new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(UserClaimTypes.UserId, "22"),
                 new Claim(UserClaimTypes.Email, "bob@gmail.com")
            }));
    }
}
