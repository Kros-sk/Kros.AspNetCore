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
            var provider = new ServiceDiscoveryProvider(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Uri uri = provider.GetService(serviceName);

            uri.AbsoluteUri.Should().Be(expectedUri);
        }

        [Theory]
        [InlineData("Users", "getAll", "http://localhost:9003/api/users", false)]
        [InlineData("Users", "getById", "http://localhost:9003/api/users/{id}", false)]
        [InlineData("Projects", "create", "http://localhost:9002/api/projects", false)]
        [InlineData("Projects", "create", "http://projects/api/projects", true)]
        public void FindPathUriByServiceAndPathName(string serviceName, string pathName, string expectedUri, bool allowHost)
        {
            var provider = new ServiceDiscoveryProvider(GetConfiguration(), new ServiceDiscoveryOptions()
            {
                AllowServiceNameAsHost = allowHost
            });

            Uri uri = provider.GetPath(serviceName, pathName);

            uri.ToString().Should().Be(expectedUri);
        }

        [Fact]
        public void FindServiceUriByNameInAnotherSection()
        {
            var provider = new ServiceDiscoveryProvider(
                GetConfiguration(),
                new ServiceDiscoveryOptions() { SectionName = "ApiServices" });

            Uri uri = provider.GetService("ToDos");

            uri.AbsoluteUri.Should().Be("http://localhost:9004/");
        }

        [Fact]
        public void ThrowExceptionIfServiceDoesntExist()
        {
            var provider = new ServiceDiscoveryProvider(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Action action = () => provider.GetService("doesntExist");

            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("Users", "create")]
        [InlineData("projects", "getById")]
        [InlineData("doesntExist", "getById")]
        public void ThrowExceptionIfServiceOrPathDoesntExist(string serviceName, string pathName)
        {
            var provider = new ServiceDiscoveryProvider(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Action action = () => provider.GetPath(serviceName, pathName);

            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(TestServiceType.UsersServiceType, "http://localhost:9003/")]
        [InlineData(TestServiceType.ProjectsServiceType, "http://localhost:9002/")]
        [InlineData(TestServiceType.AuthorizationServiceType, "https://authorizationservice.domain.com/")]
        public void FindServiceUriByType(TestServiceType serviceType, string expectedUri)
        {
            var provider = new ServiceDiscoveryProvider(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Uri uri = provider.GetService(serviceType);

            uri.AbsoluteUri.Should().Be(expectedUri);
        }

        [Theory]
        [InlineData(TestServiceType.UsersServiceType, "getAll", "http://localhost:9003/api/users")]
        [InlineData(TestServiceType.UsersServiceType, "getById", "http://localhost:9003/api/users/{id}")]
        [InlineData(TestServiceType.ProjectsServiceType, "create", "http://localhost:9002/api/projects")]
        public void FindPathUriByServiceTypeAndPathName(TestServiceType serviceType, string pathName, string expectedUri)
        {
            var provider = new ServiceDiscoveryProvider(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Uri uri = provider.GetPath(serviceType, pathName);

            uri.ToString().Should().Be(expectedUri);
        }

        [Fact]
        public void ThrowExceptionIfServiceTypeWithoutAttribute()
        {
            var provider = new ServiceDiscoveryProvider(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Action action = () => provider.GetService(TestServiceType.WithtoutParameter);

            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("test")]
        [InlineData("nonexisting")]
        public void AllowServiceNameAsHost(string serviceName)
        {
            var provider = new ServiceDiscoveryProvider(GetConfiguration(),
                new ServiceDiscoveryOptions()
                {
                    AllowServiceNameAsHost = true
                });

            Uri uri = provider.GetService(serviceName);

            uri.Scheme.Should().Be("http");
            uri.Host.Should().Be(serviceName);
            uri.Port.Should().Be(80);
        }

        private static IConfiguration GetConfiguration() =>
            new ConfigurationBuilder()
                .AddJsonFile("servicediscoveryAppsettings.json")
                .Build();
    }

    public enum TestServiceType
    {
        [ServiceName("Authorization")]
        AuthorizationServiceType,
        [ServiceName("Users")]
        UsersServiceType,
        [ServiceName("projects")]
        ProjectsServiceType,
        WithtoutParameter
    }
}
