using Xunit;
using Kros.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using FluentAssertions;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class ConfigurationBuilderExtensionsShould
    {
        [Fact]
        public void AddConfigurationSource()
        {
            // arrange
            IConfigurationBuilder config = new ConfigurationBuilder();
            HostBuilderContext builderContext = CreateHostBuilderContext();

            // act
            config.AddAzureAppConfiguration(builderContext);

            // assert
            config.Sources.Count.Should().Be(1);
        }

        private HostBuilderContext CreateHostBuilderContext()
        {
            var context = new HostBuilderContext(new Dictionary<object, object>());
            context.Configuration = GetConfiguration();
            return context;
        }

        private static IConfiguration GetConfiguration()
           => new ConfigurationBuilder().AddJsonFile("Extensions\\appsettings.configuration-test.json").Build();
    }
}
