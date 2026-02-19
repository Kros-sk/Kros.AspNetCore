using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
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
            IConfiguration configuration = GetConfiguration();
            ServiceCollection serviceCollection = new();

            serviceCollection.ConfigureOptions<TestOptions>(configuration);

            ServiceProvider provider = serviceCollection.BuildServiceProvider();

            IOptions<TestOptions> options = provider.GetService<IOptions<TestOptions>>();

            Assert.Equal(1, options.Value.Value);
        }

        [Fact]
        public void AddProxyAddressForHttpClient()
        {
            IConfiguration configuration = GetConfiguration();
            ServiceCollection serviceCollection = new();

            serviceCollection.SetProxy(configuration);

            Assert.Equal(new Uri("http://example.com:1234"), ((WebProxy)HttpClient.DefaultProxy).Address);
        }

        [Fact]
        public void AddBypassProxyOnLocalForHttpClient()
        {
            IConfiguration configuration = GetConfiguration();
            ServiceCollection serviceCollection = new();

            serviceCollection.SetProxy(configuration);

            Assert.True(((WebProxy)HttpClient.DefaultProxy).BypassProxyOnLocal);
        }

        private static IConfiguration GetConfiguration()
           => new ConfigurationBuilder().AddJsonFile("Extensions\\appsettings.configuration-test.json").Build();
    }
}
