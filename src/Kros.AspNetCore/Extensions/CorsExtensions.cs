using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Default Cors configuration helpers.
    /// </summary>
    public static class CorsExtensions
    {
        private const string AllowAnyOrigins = "AllowAnyOrigins";

        /// <summary>
        /// Adds allow all origins Cors policy with caching preflights requests with default value (1 hour).
        /// </summary>
        /// <param name="services">IoC container.</param>
        public static IServiceCollection AddAllowAnyOriginCors(this IServiceCollection services)
            => AddAllowAnyOriginCors(services, TimeSpan.FromHours(1));

        /// <summary>
        /// Adds allow all origins Cors policy with caching preflight requests with defined time.
        /// </summary>
        /// <param name="services">IoC container.</param>
        /// <param name="preflightMaxAge">Indicating the time a preflight request can be cached.</param>
        public static IServiceCollection AddAllowAnyOriginCors(this IServiceCollection services,
            TimeSpan preflightMaxAge)
            => services.AddCors(options =>
            {
                options.AddPolicy(
                    AllowAnyOrigins,
                    builder => {
                        builder
                            .SetIsOriginAllowed(origin => true)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();

                        if (preflightMaxAge > TimeSpan.Zero)
                        {
                            builder.SetPreflightMaxAge(preflightMaxAge);
                        }
                    });
            });

        /// <summary>
        /// Adds custom Cors policy with caching preflights requests with default value (1 hour).
        /// </summary>
        /// <param name="services">IoC container.</param>
        /// <param name="allowedOrigins">List of allowed origins.</param>
        /// <param name="policyName">Name of custom cors policy.</param>
        public static IServiceCollection AddCustomOriginsCorsPolicy(
            this IServiceCollection services,
            string[] allowedOrigins,
            string policyName)
            => AddCustomOriginsCorsPolicy(services, allowedOrigins, policyName, TimeSpan.FromHours(1));

        /// <summary>
        /// Adds custom Cors policy with caching preflight requests with defined time.
        /// </summary>
        /// <param name="services">IoC container.</param>
        /// <param name="allowedOrigins">List of allowed origins.</param>
        /// <param name="policyName">Name of custom cors policy.</param>
        /// <param name="preflightMaxAge">Indicating the time a preflight request can be cached.</param>
        public static IServiceCollection AddCustomOriginsCorsPolicy(
            this IServiceCollection services,
            string[] allowedOrigins,
            string policyName,
            TimeSpan preflightMaxAge)
            => services.AddCors(options =>
            {
                options.AddPolicy(
                    policyName,
                    builder => {
                        builder
                            .WithOrigins(allowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader();

                        if (preflightMaxAge > TimeSpan.Zero)
                        {
                            builder.SetPreflightMaxAge(preflightMaxAge);
                        }
                    });
            });

        /// <summary>
        /// Register usage of default cors policy.
        /// </summary>
        /// <param name="app">Application.</param>
        public static IApplicationBuilder UseAllowAllOriginsCors(this IApplicationBuilder app)
            => app.UseCors(AllowAnyOrigins);

        /// <summary>
        /// Register usage of custom cors policy.
        /// </summary>
        /// <param name="app">Application.</param>
        /// <param name="policyName">Name of custom cors policy.</param>
        public static IApplicationBuilder UseCustomOriginsCors(this IApplicationBuilder app, string policyName)
            => app.UseCors(policyName);
    }
}
