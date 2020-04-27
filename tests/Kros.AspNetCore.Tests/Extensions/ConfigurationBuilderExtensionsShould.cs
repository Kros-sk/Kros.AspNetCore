using Xunit;
using Kros.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class ConfigurationBuilderExtensionsShould
    {
        [Fact]
        public void AddAzureAppConfigurationSource()
        {
            IConfigurationBuilder config = new ConfigurationBuilder();
            HostBuilderContext builderContext = CreateHostBuilderContext();

            config.AddAzureAppConfiguration(builderContext);

            config.Sources.Count.Should().Be(1);
        }

        [Fact]
        public void AddAzureAppConfigurationSource2()
        {
            IConfigurationBuilder config = new ConfigurationBuilder();

            config.AddAzureAppConfig("Development");

            config.Sources.Count.Should().Be(1);
        }

        private HostBuilderContext CreateHostBuilderContext()
        {
            var context = new HostBuilderContext(new Dictionary<object, object>());
            context.Configuration = GetConfiguration();
            context.HostingEnvironment = Substitute.For<IHostEnvironment>();
            context.HostingEnvironment.EnvironmentName.Returns("Development");
            return context;
        }

        private static IConfiguration GetConfiguration()
           => new ConfigurationBuilder().AddJsonFile("Extensions\\appsettings.configuration-test.json").Build();
    }
}
