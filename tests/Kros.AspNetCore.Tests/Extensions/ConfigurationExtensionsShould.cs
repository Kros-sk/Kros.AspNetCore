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
            IConfiguration configuration = TestsHelper.GetConfiguration();
            TestOptions options = configuration.GetSection<TestOptions>();

            Assert.Equal(1, options.Value);
        }

        [Fact]
        public void GetAllowedOrigins()
        {
            IConfiguration configuration = TestsHelper.GetConfiguration();
            string[] allowedOrigins = configuration.GetAllowedOrigins();

            Assert.Equivalent(new[] { "*" }, allowedOrigins);
        }
    }
}
