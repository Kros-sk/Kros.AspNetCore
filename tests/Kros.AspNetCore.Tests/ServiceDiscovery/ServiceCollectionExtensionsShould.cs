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
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            serviceCollection.AddServiceDiscovery();

            IServiceDiscoveryProvider provider =  serviceCollection
                .BuildServiceProvider()
                .GetService<IServiceDiscoveryProvider>();

            provider.Should().NotBeNull();
        }

        [Fact]
        public void AddApiJwtAuthenticationShouldAddOneScheme()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddApiJwtAuthentication(new string[] { JwtAuthorizationHelper.JwtSchemeName }, GetConfiguration());
            serviceCollection.AddLogging(x => new object());
            serviceCollection.BuildServiceProvider().GetServices<JwtBearerHandler>().Count().Should().Be(1);
        }

        [Fact]
        public void AddApiJwtAuthenticationShouldAddMultipleSchemes()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddApiJwtAuthentication(
                new string[] { JwtAuthorizationHelper.JwtSchemeName, JwtAuthorizationHelper.HashJwtSchemeName },
                GetConfiguration());
            serviceCollection.AddLogging(x => new object());
            serviceCollection.BuildServiceProvider().GetServices<JwtBearerHandler>().Count().Should().Be(2);
        }

        [Fact]
        public void AddApiJwtAuthenticationRequiresAtLeastOneScheme()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.Invoking(sc =>  sc.AddApiJwtAuthentication(new string[] { }, GetConfiguration()))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddApiJwtAuthenticationIgnoresUnconfiguredSchemes()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.Invoking(sc => sc.AddApiJwtAuthentication(new string[] { "unknown" }, GetConfiguration()))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task AddApiKeyBasicAuthentication()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddAuthentication().AddApiKeyBasicAuthentication(GetApiKeyConfiguration());
            var schemeProvider = serviceCollection.BuildServiceProvider().GetRequiredService<IAuthenticationSchemeProvider>();
            var scheme = await schemeProvider.GetSchemeAsync("Basic.ApiKey");
            scheme.Should().NotBeNull();
            scheme.HandlerType.Name.Should().Be(typeof(ApiKeyBasicAuthenticationHandler).Name);
        }

        [Fact]
        public void ThrowOnAddApiKeyBasicAuthenticationWithoutConfig()
        {
            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder().Build();
            serviceCollection.AddAuthentication().Invoking(builder => builder.AddApiKeyBasicAuthentication(config))
                .Should().Throw<ArgumentNullException>();
        }

        private static IConfiguration GetConfiguration()
        {
            var cfg = @"{
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
            var cfgBuilder = new ConfigurationBuilder();
            cfgBuilder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(cfg)));
            return cfgBuilder.Build();
        }

        private static IConfiguration GetApiKeyConfiguration()
        {
            var cfg = @"{
                            ""ApiKeyBasicAuthentication"": {
                                ""ApiKey"": ""key2"",
                                ""Scheme"": ""Basic.ApiKey""
                            }
                        }";
            var cfgBuilder = new ConfigurationBuilder();
            cfgBuilder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(cfg)));
            return cfgBuilder.Build();
        }
    }
}
