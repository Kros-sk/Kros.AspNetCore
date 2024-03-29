﻿using FluentAssertions;
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
            ClaimsPrincipal user = new();

            user.GetUserId()
                .Should().Be(0);
        }

        [Fact]
        public void GetDefaultUserEmailWhenDoNotExist()
        {
            ClaimsPrincipal user = new();

            user.GetUserEmail()
                .Should().BeEmpty();
        }

        [Fact]
        public void GetCustomClaimWhenItExists()
        {
            ClaimsPrincipal user = CreateUser();

            user.GetValueFromUserClaims(CustomClaim)
                .Should().Be("something");
        }

        [Fact]
        public void GetCustomClaimWhenItDoesNotExist()
        {
            ClaimsPrincipal user = new();

            user.GetValueFromUserClaims(CustomClaim)
                .Should().Be(string.Empty);
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
