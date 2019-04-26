using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Kros.AspNetCore
{
    /// <summary>
    /// Base class for Startup classes in services.
    /// </summary>
    public abstract class BaseStartup
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="env">Information about the web hosting environment.</param>
        public BaseStartup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile($"appsettings.local.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        /// <summary>
        /// Application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }
    }
}
