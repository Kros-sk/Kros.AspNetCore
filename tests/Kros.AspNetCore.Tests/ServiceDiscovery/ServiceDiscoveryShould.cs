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
            ServiceDiscoveryProvider provider = new(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Uri uri = provider.GetService(serviceName);

            Assert.Equal(expectedUri, uri.AbsoluteUri);
        }

        [Theory]
        [InlineData("Users", "getAll", "http://localhost:9003/api/users", false)]
        [InlineData("Users", "getById", "http://localhost:9003/api/users/{id}", false)]
        [InlineData("Projects", "create", "http://localhost:9002/api/projects", false)]
        [InlineData("Projects", "create", "http://projects/api/projects", true)]
        [InlineData("Catalog", "create", "http://localhost:9004/api/catalog", true)]
        public void FindPathUriByServiceAndPathName(string serviceName, string pathName, string expectedUri, bool allowHost)
        {
            ServiceDiscoveryProvider provider = new(GetConfiguration(), new ServiceDiscoveryOptions()
            {
                AllowServiceNameAsHost = allowHost
            });

            Uri uri = provider.GetPath(serviceName, pathName);

            Assert.Equal(expectedUri, uri.ToString());
        }

        [Fact]
        public void FindServiceUriByNameInAnotherSection()
        {
            ServiceDiscoveryProvider provider = new(
                GetConfiguration(),
                new ServiceDiscoveryOptions() { SectionName = "ApiServices" });

            Uri uri = provider.GetService("ToDos");

            Assert.Equal("http://localhost:9004/", uri.AbsoluteUri);
        }

        [Fact]
        public void ThrowExceptionIfServiceDoesntExist()
        {
            ServiceDiscoveryProvider provider = new(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Action action = () => provider.GetService("doesntExist");

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData("Users", "create")]
        [InlineData("projects", "getById")]
        [InlineData("doesntExist", "getById")]
        public void ThrowExceptionIfServiceOrPathDoesntExist(string serviceName, string pathName)
        {
            ServiceDiscoveryProvider provider = new(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Action action = () => provider.GetPath(serviceName, pathName);

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData(TestServiceType.UsersServiceType, "http://localhost:9003/")]
        [InlineData(TestServiceType.ProjectsServiceType, "http://localhost:9002/")]
        [InlineData(TestServiceType.AuthorizationServiceType, "https://authorizationservice.domain.com/")]
        public void FindServiceUriByType(TestServiceType serviceType, string expectedUri)
        {
            ServiceDiscoveryProvider provider = new(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Uri uri = provider.GetService(serviceType);

            Assert.Equal(expectedUri, uri.AbsoluteUri);
        }

        [Theory]
        [InlineData(TestServiceType.UsersServiceType, "getAll", "http://localhost:9003/api/users")]
        [InlineData(TestServiceType.UsersServiceType, "getById", "http://localhost:9003/api/users/{id}")]
        [InlineData(TestServiceType.ProjectsServiceType, "create", "http://localhost:9002/api/projects")]
        public void FindPathUriByServiceTypeAndPathName(TestServiceType serviceType, string pathName, string expectedUri)
        {
            ServiceDiscoveryProvider provider = new(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Uri uri = provider.GetPath(serviceType, pathName);

            Assert.Equal(expectedUri, uri.ToString());
        }

        [Fact]
        public void ThrowExceptionIfServiceTypeWithoutAttribute()
        {
            ServiceDiscoveryProvider provider = new(GetConfiguration(), ServiceDiscoveryOptions.Default);

            Action action = () => provider.GetService(TestServiceType.WithtoutParameter);

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData("test")]
        [InlineData("nonexisting")]
        public void AllowServiceNameAsHost(string serviceName)
        {
            ServiceDiscoveryProvider provider = new(GetConfiguration(),
                new ServiceDiscoveryOptions()
                {
                    AllowServiceNameAsHost = true
                });

            Uri uri = provider.GetService(serviceName);

            Assert.Equal("http", uri.Scheme);
            Assert.Equal(serviceName, uri.Host);
            Assert.Equal(80, uri.Port);
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
