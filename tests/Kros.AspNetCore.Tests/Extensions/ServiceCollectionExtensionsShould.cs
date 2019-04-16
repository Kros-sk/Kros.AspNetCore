using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        private static IConfiguration GetConfiguration()
           => new ConfigurationBuilder().AddJsonFile("Extensions\\appsettings.configuration-test.json").Build();
    }
}
