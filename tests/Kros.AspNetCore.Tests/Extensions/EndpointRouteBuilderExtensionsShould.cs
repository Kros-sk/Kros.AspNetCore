using System;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Kros.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class EndpointRouteBuilderExtensionsShould
    {
        [Fact]
        public void ThrowExceptionIfHttpContextDispatcherOptionsAreMissing()
        {
            IConfiguration configuration = GetBadConfiguration();
            IEndpointRouteBuilder endpoints = Substitute.For<IEndpointRouteBuilder>();

            Action action = () => endpoints.MapSignalRHubWithOptions<Hub>(configuration, "Route");

            action.Should().Throw<InvalidOperationException>(Arg.Any<string>());
        }

        [Fact]
        public void NotThrowExceptionIfHttpContextDispatcherOptionsAreSet()
        {
            IConfiguration configuration = GetConfiguration();
            IEndpointRouteBuilder endpoints = Substitute.For<IEndpointRouteBuilder>();

            Action action = () => endpoints.MapSignalRHubWithOptions<Hub>(configuration, "Route");

            action.Should().NotThrow<InvalidOperationException>(Arg.Any<string>());
        }

        [Fact]
        public void HaveOptionsCorrectlySet()
        {
            IConfiguration configuration = GetConfiguration();
            HttpConnectionDispatcherOptions options = new HttpConnectionDispatcherOptions();

            HttpConnectionDispatcherOptions fromCfg = configuration.GetSection<HttpConnectionDispatcherOptions>();

            options.ApplicationMaxBufferSize.Should().Be(fromCfg.ApplicationMaxBufferSize);
            options.TransportMaxBufferSize.Should().NotBe(fromCfg.TransportMaxBufferSize);
        }

        private static IConfiguration GetBadConfiguration()
           => new ConfigurationBuilder().AddJsonFile(Path.Combine("Extensions", "appsettings.configuration-test-signalr-bad.json")).Build();

        private static IConfiguration GetConfiguration()
            => new ConfigurationBuilder().AddJsonFile(Path.Combine("Extensions", "appsettings.configuration-test.json")).Build();
    }
}
