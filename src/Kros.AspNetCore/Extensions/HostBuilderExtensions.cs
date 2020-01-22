using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Host builder extensions.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Add local.json configuration.
        /// </summary>
        /// <param name="hostBuilder">Host builder.</param>
        /// <returns>The same instance of the Microsoft.Extensions.Hosting.IHostBuilder for chaining.</returns>
        public static IHostBuilder AddLocalConfiguration(this IHostBuilder hostBuilder)
            => hostBuilder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                      .AddJsonFile("appsettings.local.json", optional: true);
            });
    }
}
