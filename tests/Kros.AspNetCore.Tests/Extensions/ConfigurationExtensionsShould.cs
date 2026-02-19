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
            IConfiguration configuration = GetConfiguration();
            TestOptions options = configuration.GetSection<TestOptions>();

            Assert.Equal(1, options.Value);
        }

        [Fact]
        public void GetAllowedOrigins()
        {
            IConfiguration configuration = GetConfiguration();
            string[] allowedOrigins = configuration.GetAllowedOrigins();

            Assert.Equivalent(new[] { "*" }, allowedOrigins);
        }

        private static IConfiguration GetConfiguration()
           => new ConfigurationBuilder().AddJsonFile("Extensions\\appsettings.configuration-test.json").Build();
    }
}
