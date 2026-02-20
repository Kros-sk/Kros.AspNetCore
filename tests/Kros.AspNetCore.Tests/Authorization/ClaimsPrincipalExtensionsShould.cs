using Kros.AspNetCore.Authorization;
using System.Security.Claims;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization
{
    public class ClaimsPrincipalExtensionsShould
    {
        private const string CustomClaim = "CustomClaim";

        [Fact]
        public void GetUserIdWhenExist()
        {
            ClaimsPrincipal user = CreateUser();

            Assert.Equal(22, user.GetUserId());
        }

        [Fact]
        public void GetEmailWhenExist()
        {
            ClaimsPrincipal user = CreateUser();

            Assert.Equal("bob@gmail.com", user.GetUserEmail());
        }

        [Fact]
        public void GetDefaultUserIdWhenDoNotExist()
        {
            ClaimsPrincipal user = new();

            Assert.Equal(0, user.GetUserId());
        }

        [Fact]
        public void GetDefaultUserEmailWhenDoNotExist()
        {
            ClaimsPrincipal user = new();

            Assert.Empty(user.GetUserEmail());
        }

        [Fact]
        public void GetCustomClaimWhenItExists()
        {
            ClaimsPrincipal user = CreateUser();

            Assert.Equal("something", user.GetValueFromUserClaims(CustomClaim));
        }

        [Fact]
        public void GetCustomClaimWhenItDoesNotExist()
        {
            ClaimsPrincipal user = new();

            Assert.Equal(string.Empty, user.GetValueFromUserClaims(CustomClaim));
        }

        private static ClaimsPrincipal CreateUser()
            => new(new ClaimsIdentity(new Claim[]
            {
                 new Claim(UserClaimTypes.UserId, "22"),
                 new Claim(ClaimTypes.Email, "bob@gmail.com"),
                 new Claim(CustomClaim, "something")
            }));
    }
}
