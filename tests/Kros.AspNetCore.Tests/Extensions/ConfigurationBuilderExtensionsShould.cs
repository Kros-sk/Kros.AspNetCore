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

            AppConfigOptions expectedAppConfig = new()
            {
                Endpoint = "https://example.azconfig.io",
                IdentityClientId = "Ipsum",
                UseFeatureFlags = true,
                RefreshInterval = TimeSpan.FromSeconds(75),
                SentinelKey = "Lorem"
            };
            expectedAppConfig.Settings.AddRange(new[] { "Example1", "Example2" });

            Assert.Equivalent(expectedAppConfig, actualAppConfig);
        }

        [Fact]
        public void LoadEmptyAppConfigOptions()
        {
            IConfiguration cfg = new ConfigurationBuilder().Build();
            AppConfigOptions actualAppConfig = new();
            cfg.Bind("AppConfig", actualAppConfig);

            AppConfigOptions expectedAppConfig = new()
            {
                Endpoint = "",
                IdentityClientId = "",
                UseFeatureFlags = false,
                RefreshInterval = TimeSpan.Zero,
                SentinelKey = ""
            };

            Assert.Equivalent(expectedAppConfig, actualAppConfig);
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

            Assert.Equal(2, config.Sources.Count);
        }

        [Fact]
        public void NotAddAzureAppConfigurationSourceIfEndpointNotDefined1()
        {
            IConfigurationBuilder config = new ConfigurationBuilder();
            HostBuilderContext builderContext = CreateHostBuilderContext();

            config.AddAzureAppConfig(builderContext);

            Assert.Empty(config.Sources);
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

            Assert.Equal(2, config.Sources.Count);
        }

        [Fact]
        public void NotAddAzureAppConfigurationSourceIfEndpointNotDefined()
        {
            IConfigurationBuilder config = new ConfigurationBuilder();

            config.AddAzureAppConfig("Development");

            Assert.Empty(config.Sources);
        }

        #endregion

        #region Key vault

        [Fact]
        public void LoadKeyVaultOptions()
        {
            IConfiguration cfg = GetConfiguration();
            KeyVaultOptions actualKv = new();
            cfg.Bind("KeyVault", actualKv);

            KeyVaultOptions expectedKv = new()
            {
                Name = "example-kv",
                IdentityClientId = "Lorem",
                ReloadInterval = TimeSpan.FromSeconds((2 * 60) + 15)
            };
            expectedKv.Prefixes.AddRange(new[] { "Example1", "Example2" });

            Assert.Equivalent(expectedKv, actualKv);
        }

        [Fact]
        public void LoadEmptyKeyVaultOptions()
        {
            IConfiguration cfg = new ConfigurationBuilder().Build();
            KeyVaultOptions actualKv = new();
            cfg.Bind("KeyVault", actualKv);

            KeyVaultOptions expectedKv = new()
            {
                Name = string.Empty,
                IdentityClientId = string.Empty
            };

            Assert.Equivalent(expectedKv, actualKv);
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

            Assert.Single(cfgBuilder.Sources);
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

            Assert.Empty(cfgBuilder.Sources);
        }

        #endregion

        #region Helpers

        private static HostBuilderContext CreateHostBuilderContext()
        {
            HostBuilderContext context = new(new Dictionary<object, object>());
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
