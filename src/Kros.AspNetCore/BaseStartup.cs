using Kros.AspNetCore.Configuration;
using Kros.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        /// <param name="configuration">Application configuration.</param>
        /// <param name="env">Information about the web hosting environment.</param>
        protected BaseStartup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        /// <summary>
        /// Application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Application environment.
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// Application services configuration.
        /// </summary>
        /// <param name="services">IoC container.</param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            if (Environment.IsTestOrDevelopment())
            {
                services.AddAllowAnyOriginCors();
            }
            else
            {
                string[] allowedOrigins = Configuration.GetAllowedOrigins();
                services.AddCustomOriginsCorsPolicy(allowedOrigins, CorsOptions.CorsPolicyName);
            }
        }

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public virtual void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            if (Environment.IsTestOrDevelopment())
            {
                app.UseAllowAllOriginsCors();
            }
            else
            {
                app.UseCustomOriginsCors(CorsOptions.CorsPolicyName);
            }
        }
    }
}
