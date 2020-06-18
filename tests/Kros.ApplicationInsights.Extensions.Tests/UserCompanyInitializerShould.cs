using FluentAssertions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class UserCompanyInitializerShould
    {
        [Fact]
        public void AddUserAndCompanyToTelemetry()
        {
            var initializer = new UserCompanyInitializer(FakeHttpContextAccessor(true));
            ITelemetry telemetry = FakeTelemetry();
            initializer.Initialize(telemetry);

            (telemetry as RequestTelemetry).Properties["company_id"].Should().Be("66");
            (telemetry as RequestTelemetry).Properties["user_id"].Should().Be("99");
        }

        [Fact]
        public void NoUserAndCompanyIfUserClaimsIsEmpty()
        {
            var initializer = new UserCompanyInitializer(FakeHttpContextAccessor(false));
            ITelemetry telemetry = FakeTelemetry();

            initializer.Initialize(telemetry);

            (telemetry as RequestTelemetry).Properties.Should().NotContainKey("company_id");
            (telemetry as RequestTelemetry).Properties.Should().NotContainKey("user_id");
        }

        private ITelemetry FakeTelemetry()
        {
            return new RequestTelemetry();
        }

        private IHttpContextAccessor FakeHttpContextAccessor(bool addClaims)
        {
            var httpContext = new DefaultHttpContext();
            IHttpContextAccessor context = new HttpContextAccessor()
            {
                HttpContext = httpContext
            };
            if (addClaims)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim("company_id", "66"));
                claims.Add(new Claim("user_id", "99"));
                context.HttpContext.User.Identities.FirstOrDefault().AddClaims(claims);
            }
            return context;
        }
    }
}
