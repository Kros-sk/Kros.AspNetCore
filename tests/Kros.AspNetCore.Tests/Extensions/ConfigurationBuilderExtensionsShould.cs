using FluentAssertions;
using Kros.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class ConfigurationBuilderExtensionsShould
    {
        #region App configuration

        [Fact]
        public void LoadAppConfigOptions()
        {
            IConfiguration cfg = GetConfiguration();
            AppConfigOptions actualAppConfig = new();
            cfg.Bind("AppConfig", actualAppConfig);

            var expectedAppConfig = new AppConfigOptions
            {
                Endpoint = "https://example.azconfig.io",
                IdentityClientId = "Ipsum",
                UseFeatureFlags = true,
                RefreshInterval = TimeSpan.FromSeconds(75),
                SentinelKey = "Lorem"
            };
            expectedAppConfig.Settings.AddRange(new[] { "Example1", "Example2" });

            actualAppConfig.Should().BeEquivalentTo(expectedAppConfig);
        }

        [Fact]
        public void LoadEmptyAppConfigOptions()
        {
            IConfiguration cfg = new ConfigurationBuilder().Build();
            AppConfigOptions actualAppConfig = new();
            cfg.Bind("AppConfig", actualAppConfig);

            var expectedAppConfig = new AppConfigOptions
            {
                Endpoint = "",
                IdentityClientId = "",
                UseFeatureFlags = false,
                RefreshInterval = TimeSpan.Zero,
                SentinelKey = ""
            };

            actualAppConfig.Should().BeEquivalentTo(expectedAppConfig);
        }

        [Fact]
        public void AddAzureAppConfigurationSourceIfEndpointDefined1()
        {
            IConfigurationBuilder config = new ConfigurationBuilder();
            HostBuilderContext builderContext = CreateHostBuilderContext();
            config.AddInMemoryCollection(
                new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("AppConfig:Endpoint", "endpoint")
                });

            config.AddAzureAppConfig(builderContext);

            config.Sources.Count.Should().Be(2);
        }

        [Fact]
        public void NotAddAzureAppConfigurationSourceIfEndpointNotDefined1()
        {
            IConfigurationBuilder config = new ConfigurationBuilder();
            HostBuilderContext builderContext = CreateHostBuilderContext();

            config.AddAzureAppConfig(builderContext);

            config.Sources.Count.Should().Be(0);
        }

        [Fact]
        public void AddAzureAppConfigurationSourceIfEndpointDefined()
        {
            IConfigurationBuilder config = new ConfigurationBuilder();
            config.AddInMemoryCollection(
                new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("AppConfig:Endpoint", "endpoint")
                });

            config.AddAzureAppConfig("Development");

            config.Sources.Count.Should().Be(2);
        }

        [Fact]
        public void NotAddAzureAppConfigurationSourceIfEndpointNotDefined()
        {
            IConfigurationBuilder config = new ConfigurationBuilder();

            config.AddAzureAppConfig("Development");

            config.Sources.Count.Should().Be(0);
        }

        #endregion

        #region Key vault

        [Fact]
        public void LoadKeyVaultOptions()
        {
            IConfiguration cfg = GetConfiguration();
            KeyVaultOptions actualKv = new();
            cfg.Bind("KeyVault", actualKv);

            var expectedKv = new KeyVaultOptions
            {
                Name = "example-kv",
                IdentityClientId = "Lorem",
                ReloadInterval = TimeSpan.FromSeconds((2 * 60) + 15)
            };
            expectedKv.Prefixes.AddRange(new[] { "Example1", "Example2" });

            actualKv.Should().BeEquivalentTo(expectedKv);
        }

        [Fact]
        public void LoadEmptyKeyVaultOptions()
        {
            IConfiguration cfg = new ConfigurationBuilder().Build();
            KeyVaultOptions actualKv = new();
            cfg.Bind("KeyVault", actualKv);

            var expectedKv = new KeyVaultOptions
            {
                Name = string.Empty,
                IdentityClientId = string.Empty
            };

            actualKv.Should().BeEquivalentTo(expectedKv);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("lorem")]
        public void ShouldAddKeyVaultSourceIfNameIsPresent(string prefix)
        {
            IConfigurationBuilder cfgBuilder = new ConfigurationBuilder();
            cfgBuilder.AddAzureKeyVault(options =>
            {
                options.Name = "example-kv";
                if (prefix is not null)
                {
                    options.Prefixes.Add(prefix);
                }
            });

            cfgBuilder.Sources.Should().HaveCount(1);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("lorem")]
        public void ShouldNotAddKeyVaultSourceIfNameIsNotPresent(string prefix)
        {
            IConfigurationBuilder cfgBuilder = new ConfigurationBuilder();
            cfgBuilder.AddAzureKeyVault(options =>
            {
                options.Name = "";
                if (prefix is not null)
                {
                    options.Prefixes.Add(prefix);
                }
            });

            cfgBuilder.Sources.Should().HaveCount(0);
        }

        #endregion

        #region Helpers

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

        #endregion
    }
}
