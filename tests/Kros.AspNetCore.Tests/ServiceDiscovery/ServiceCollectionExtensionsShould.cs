using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    }
}
