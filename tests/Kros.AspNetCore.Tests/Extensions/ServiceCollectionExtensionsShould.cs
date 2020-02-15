using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using Xunit;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class ServiceCollectionExtensionsShould
    {
        #region Test class

        class TestOptions
        {
            public int Value { get; set; }
        }

        #endregion

        [Fact]
        public void ConfigureOptionsWithDefaultName()
        {
            var configuration = GetConfiguration();
            var serviceCollection = new ServiceCollection();

            serviceCollection.ConfigureOptions<TestOptions>(configuration);

            var provider = serviceCollection.BuildServiceProvider();

            var options = provider.GetService<IOptions<TestOptions>>();

            options.Value.Value.Should().Be(1);
        }

        [Fact]
        public void AddProxyAddressForHttpClient()
        {
            var configuration = GetConfiguration();
            var serviceCollection = new ServiceCollection();

            serviceCollection.SetProxy(configuration);

            ((WebProxy)HttpClient.DefaultProxy).Address.Should().Be("http://example.com:1234");
        }

        [Fact]
        public void AddBypassProxyOnLocalForHttpClient()
        {
            var configuration = GetConfiguration();
            var serviceCollection = new ServiceCollection();

            serviceCollection.SetProxy(configuration);

            ((WebProxy)HttpClient.DefaultProxy).BypassProxyOnLocal.Should().Be(true);
        }

        private static IConfiguration GetConfiguration()
           => new ConfigurationBuilder().AddJsonFile("Extensions\\appsettings.configuration-test.json").Build();
    }
}
