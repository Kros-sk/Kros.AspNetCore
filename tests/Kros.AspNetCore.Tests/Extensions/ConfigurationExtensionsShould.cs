using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class ConfigurationExtensionsShould
    {

        #region Test class

        class TestOptions
        {
            public int Value { get; set; }
        }

        #endregion

        [Fact]
        public void GetOptionsByType()
        {
            var configuration = GetConfiguration();
            var options = configuration.GetSection<TestOptions>();

            options.Value.Should().Be(1);
        }

        private static IConfiguration GetConfiguration()
           => new ConfigurationBuilder().AddJsonFile("Extensions\\appsettings.configuration-test.json").Build();
    }
}
