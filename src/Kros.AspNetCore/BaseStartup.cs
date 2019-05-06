using Kros.AspNetCore.Extensions;
using Kros.AspNetCore.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            Environment = env;
        }

        /// <summary>
        /// Application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Application environment.
        /// </summary>
        public IHostingEnvironment Environment { get; }

        /// <summary>
        /// Application services configuration.
        /// </summary>
        /// <param name="services">IoC container.</param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            if (Environment.IsDevelopment())
            {
                services.AddAllowAnyOriginCors();
            }
            else
            {
                var allowedOrigins = Configuration.GetSection(CorsOptions.CorsSectionName).Get<string[]>();
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
            if (Environment.IsDevelopment())
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
