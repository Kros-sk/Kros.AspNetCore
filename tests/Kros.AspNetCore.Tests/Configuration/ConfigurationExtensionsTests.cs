using FluentAssertions;
using Kros.AspNetCore.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;
using Xunit;

namespace Kros.AspNetCore.Tests.Configuration
{
    public class ConfigurationExtensionsTests
    {
        const string TestSettingsJson = @"
{
  ""Test"": {
    ""StringValue"": ""Lorem Ipsum"",
    ""IntValue"": 42
  }
}";

        class TestSettings
        {
            public string StringValue { get; set; }
            public int IntValue { get; set; }
        }

        class ValidatableTestSettings : IValidatable
        {
            public string StringValue { get; set; }
            public int IntValue { get; set; }

            public void Validate()
            {
            }
        }

        [Fact]
        public void ConfigureOptionsRegistersClassIntoDiContainer()
        {
            IConfiguration cfg = new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(TestSettingsJson)))
                .Build();
            var services = new ServiceCollection();

            services.ConfigureOptions<TestSettings>(cfg);

            ServiceProvider provider = services.BuildServiceProvider();
            TestSettings settings = provider.GetRequiredService<TestSettings>();

            settings.Should().NotBeNull();
            settings.Should().BeEquivalentTo(new TestSettings
            {
                StringValue = "Lorem Ipsum",
                IntValue = 42
            });
        }

        [Fact]
        public void ConfigureOptionsRegistersClassAsIValidatableIntoDiContainer()
        {
            IConfiguration cfg = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services.ConfigureOptions<ValidatableTestSettings>(cfg);
            ServiceProvider provider = services.BuildServiceProvider();

            IValidatable settings = provider.GetRequiredService<IValidatable>();

            settings.Should().NotBeNull();
            settings.Should().BeOfType<ValidatableTestSettings>();
        }
    }
}
