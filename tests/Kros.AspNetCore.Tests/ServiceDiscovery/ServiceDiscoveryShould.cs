using FluentAssertions;
using Kros.AspNetCore.ServiceDiscovery;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

namespace Kros.AspNetCore.Tests.ServiceDiscovery
{
    public class ServiceDiscoveryShould
    {
        [Theory]
        [InlineData("Users", "http://localhost:9003/")]
        [InlineData("Projects", "http://localhost:9002/")]
        [InlineData("Authorization", "https://authorizationservice.domain.com/")]
        public void FindServiceUriByName(string serviceName, string expectedUri)
        {
            var provider = new ServiceDiscoveryProvider(GetConfiguration(), ServiceDiscoveryOption.Default);

            Uri uri = provider.GetService(serviceName);

            uri.AbsoluteUri.Should().Be(expectedUri);
        }

        [Theory]
        [InlineData("Users", "getAll", "http://localhost:9003/api/users")]
        [InlineData("Users", "getById", "http://localhost:9003/api/users/{id}")]
        [InlineData("Projects", "create", "http://localhost:9002/api/projects")]
        public void FindPathUriByServiceAndPathName(string serviceName, string pathName, string expectedUri)
        {
            var provider = new ServiceDiscoveryProvider(GetConfiguration(), ServiceDiscoveryOption.Default);

            Uri uri = provider.GetPath(serviceName, pathName);

            uri.ToString().Should().Be(expectedUri);
        }

        private static IConfiguration GetConfiguration() =>
            new ConfigurationBuilder()
                .AddJsonFile("servicediscoveryAppsettings.json")
                .Build();
    }
}
