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

            Assert.NotNull(provider);
        }

        [Fact]
        public void AddApiJwtAuthenticationShouldAddOneScheme()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddApiJwtAuthentication(new string[] { JwtAuthorizationHelper.JwtSchemeName }, GetConfiguration());
            serviceCollection.AddLogging();
            Assert.Single(serviceCollection.BuildServiceProvider().GetServices<JwtBearerHandler>());
        }

        [Fact]
        public void AddApiJwtAuthenticationShouldAddMultipleSchemes()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddApiJwtAuthentication(
                new string[] { JwtAuthorizationHelper.JwtSchemeName, JwtAuthorizationHelper.HashJwtSchemeName },
                GetConfiguration());
            serviceCollection.AddLogging();
            Assert.Equal(2, serviceCollection.BuildServiceProvider().GetServices<JwtBearerHandler>().Count());
        }

        [Fact]
        public void AddApiJwtAuthenticationRequiresAtLeastOneScheme()
        {
            ServiceCollection serviceCollection = new();
            Assert.Throws<ArgumentException>(() => serviceCollection.AddApiJwtAuthentication(Array.Empty<string>(), GetConfiguration()));
        }

        [Fact]
        public void AddApiJwtAuthenticationIgnoresUnconfiguredSchemes()
        {
            ServiceCollection serviceCollection = new();
            Assert.Throws<ArgumentException>(() => serviceCollection.AddApiJwtAuthentication(new string[] { "unknown" }, GetConfiguration()));
        }

        [Fact]
        [Obsolete("AddApiKeyBasicAuthentication is obsolete.")]
        public async Task AddSingleApiKeyBasicAuthentication()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddAuthentication().AddApiKeyBasicAuthentication(GetSingleApiKeyConfiguration());
            IAuthenticationSchemeProvider schemeProvider = serviceCollection.BuildServiceProvider()
                .GetRequiredService<IAuthenticationSchemeProvider>();
            AuthenticationScheme scheme = await schemeProvider.GetSchemeAsync("Basic.ApiKey");
            Assert.NotNull(scheme);
            Assert.Equal(typeof(ApiKeyBasicAuthenticationHandler).Name, scheme.HandlerType.Name);
        }

        [Fact]
        public async Task AddMultipleApiKeyBasicAuthenticationSchemes()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddAuthentication().AddApiKeyBasicAuthenticationSchemes(GetMultipleApiKeyConfiguration());
            IAuthenticationSchemeProvider schemeProvider = serviceCollection.BuildServiceProvider()
                .GetRequiredService<IAuthenticationSchemeProvider>();

            AuthenticationScheme scheme = await schemeProvider.GetSchemeAsync("Basic.ApiKey");
            Assert.NotNull(scheme);
            Assert.Equal(typeof(ApiKeyBasicAuthenticationHandler).Name, scheme.HandlerType.Name);

            AuthenticationScheme anotherScheme = await schemeProvider.GetSchemeAsync("Another.ApiKey");
            Assert.NotNull(anotherScheme);
            Assert.Equal(typeof(ApiKeyBasicAuthenticationHandler).Name, anotherScheme.HandlerType.Name);
        }

        [Fact]
        [Obsolete("AddApiKeyBasicAuthentication is obsolete.")]
        public void ThrowOnAddApiKeyBasicAuthenticationWithoutConfig()
        {
            ServiceCollection serviceCollection = new();
            IConfigurationRoot config = new ConfigurationBuilder().Build();
            AuthenticationBuilder builder = serviceCollection.AddAuthentication();
            Assert.Throws<ArgumentNullException>(() => builder.AddApiKeyBasicAuthentication(config));
        }

        [Fact]
        public void ThrowOnAddApiKeyBasicAuthenticationSchemesWithoutConfig()
        {
            ServiceCollection serviceCollection = new();
            IConfigurationRoot config = new ConfigurationBuilder().Build();
            AuthenticationBuilder builder = serviceCollection.AddAuthentication();
            Assert.Throws<ArgumentNullException>(() => builder.AddApiKeyBasicAuthenticationSchemes(config));
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

        private static IConfiguration GetSingleApiKeyConfiguration()
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

        private static IConfiguration GetMultipleApiKeyConfiguration()
        {
            string cfg = @"{
                            ""ApiKeyBasicAuthentication"": {
                                ""Schemes"":
                                    {
                                        ""Basic.ApiKey"": ""key1"",
                                        ""Another.ApiKey"": ""key2""
                                    }
                            }
                        }";
            ConfigurationBuilder cfgBuilder = new();
            cfgBuilder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(cfg)));
            return cfgBuilder.Build();
        }
    }
}
