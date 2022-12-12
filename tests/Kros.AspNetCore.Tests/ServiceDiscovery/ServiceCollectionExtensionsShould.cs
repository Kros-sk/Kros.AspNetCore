using FluentAssertions;
using Kros.AspNetCore.Authentication;
using Kros.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.ServiceDiscovery
{
    public class ServiceCollectionExtensionsShould
    {
        [Fact]
        public void RegisterServiceDiscoveryProvider()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            serviceCollection.AddServiceDiscovery();

            IServiceDiscoveryProvider provider = serviceCollection
                .BuildServiceProvider()
                .GetService<IServiceDiscoveryProvider>();

            provider.Should().NotBeNull();
        }

        [Fact]
        public void AddApiJwtAuthenticationShouldAddOneScheme()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddApiJwtAuthentication(new string[] { JwtAuthorizationHelper.JwtSchemeName }, GetConfiguration());
            serviceCollection.AddLogging();
            serviceCollection.BuildServiceProvider().GetServices<JwtBearerHandler>().Count().Should().Be(1);
        }

        [Fact]
        public void AddApiJwtAuthenticationShouldAddMultipleSchemes()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddApiJwtAuthentication(
                new string[] { JwtAuthorizationHelper.JwtSchemeName, JwtAuthorizationHelper.HashJwtSchemeName },
                GetConfiguration());
            serviceCollection.AddLogging();
            serviceCollection.BuildServiceProvider().GetServices<JwtBearerHandler>().Count().Should().Be(2);
        }

        [Fact]
        public void AddApiJwtAuthenticationRequiresAtLeastOneScheme()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.Invoking(sc => sc.AddApiJwtAuthentication(Array.Empty<string>(), GetConfiguration()))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddApiJwtAuthenticationIgnoresUnconfiguredSchemes()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.Invoking(sc => sc.AddApiJwtAuthentication(new string[] { "unknown" }, GetConfiguration()))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task AddApiKeyBasicAuthentication()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddAuthentication().AddApiKeyBasicAuthentication(GetApiKeyConfiguration());
            IAuthenticationSchemeProvider schemeProvider = serviceCollection.BuildServiceProvider()
                .GetRequiredService<IAuthenticationSchemeProvider>();
            AuthenticationScheme scheme = await schemeProvider.GetSchemeAsync("Basic.ApiKey");
            scheme.Should().NotBeNull();
            scheme.HandlerType.Name.Should().Be(typeof(ApiKeyBasicAuthenticationHandler).Name);
        }

        [Fact]
        public void ThrowOnAddApiKeyBasicAuthenticationWithoutConfig()
        {
            ServiceCollection serviceCollection = new();
            IConfigurationRoot config = new ConfigurationBuilder().Build();
            serviceCollection.AddAuthentication().Invoking(builder => builder.AddApiKeyBasicAuthentication(config))
                .Should().Throw<ArgumentNullException>();
        }

        private static IConfiguration GetConfiguration()
        {
            string cfg = @"{
                    ""ApiJwtAuthorization"": {
                        ""Schemes"": [
                          {
                            ""SchemeName"": ""JwtAuthorization"",
                            ""JwtSecret"": ""secret1"",
                            ""RequireHttpsMetadata"": true
                          },
                          {
                            ""SchemeName"": ""JwtHashAuthorization"",
                            ""JwtSecret"": ""secret2"",
                            ""RequireHttpsMetadata"": true
                          }
                        ]
                       }
                    }";
            ConfigurationBuilder cfgBuilder = new();
            cfgBuilder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(cfg)));
            return cfgBuilder.Build();
        }

        private static IConfiguration GetApiKeyConfiguration()
        {
            string cfg = @"{
                            ""ApiKeyBasicAuthentication"": {
                                ""ApiKey"": ""key2"",
                                ""Scheme"": ""Basic.ApiKey""
                            }
                        }";
            ConfigurationBuilder cfgBuilder = new();
            cfgBuilder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(cfg)));
            return cfgBuilder.Build();
        }
    }
}
