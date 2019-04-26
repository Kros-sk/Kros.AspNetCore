using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Default Cors configuration helpers.
    /// </summary>
    public static class CorsExtensions
    {
        private const string AllowAnyOrigins = "AllowAnyOrigins";

        /// <summary>
        /// Adds allow all origins Cors policy.
        /// </summary>
        /// <param name="services">IoC container.</param>
        public static IServiceCollection AddAllowAnyOriginCors(this IServiceCollection services)
            => services.AddCors(options =>
            {
                options.AddPolicy(
                    AllowAnyOrigins,
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

        /// <summary>
        /// Adds custom Cors policy.
        /// </summary>
        /// <param name="services">IoC container.</param>
        /// <param name="allowedOrigins">List of allowed origins.</param>
        /// <param name="policyName">Name of custom cors policy.</param>
        public static IServiceCollection AddCustomOriginsCorsPolicy(
            this IServiceCollection services,
            string[] allowedOrigins,
            string policyName)
            => services.AddCors(options =>
            {
                options.AddPolicy(
                    policyName,
                    builder => builder
                        .WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
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
