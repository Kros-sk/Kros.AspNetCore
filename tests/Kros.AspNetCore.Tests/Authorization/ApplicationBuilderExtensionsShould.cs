using FluentAssertions;
using Kros.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace Kros.AspNetCore.Tests.Authorization
{
    public class ApplicationBuilderExtensionsShould
    {
        [Fact]
        public void RegisterGatewayJwtAuthorizationMiddlewareToPipeline()
        {
            IConfiguration config = GetConfiguration(new Dictionary<string, string>()
            {
                {
                    "GatewayJwtAuthorization:AuthorizationUrl", "urlToAuthorizationService"
                }
            });
            IApplicationBuilder builder = Substitute.For<IApplicationBuilder>();

            builder.UseGatewayJwtAuthorization(config);

            builder.Received().Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
        }

        [Fact]
        public void RegisterGatewayJwtAuthorizationMiddlewareToPipelineWithConfigMethod()
        {
            IApplicationBuilder builder = Substitute.For<IApplicationBuilder>();

            builder.UseGatewayJwtAuthorization(() => new GatewayJwtAuthorizationOptions());

            builder.Received().Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
        }

        [Fact]
        public void ThrowWhenGatewayJwtAuthorizationSectionIsMising()
        {
            IConfiguration config = GetConfiguration(new Dictionary<string, string>() { });
            IApplicationBuilder builder = Substitute.For<IApplicationBuilder>();

            Action action = () => builder.UseGatewayJwtAuthorization(config);

            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*GatewayJwtAuthorization*");
        }

        private static IConfiguration GetConfiguration(Dictionary<string, string> values) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
    }
}
