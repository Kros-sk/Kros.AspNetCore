using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Extensions for <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Add gateway authentication middleware to request pipeline.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/> where middleware is added.</param>
        /// <param name="configuration">Configuration from which the options are loaded.
        /// Configuration must contains GatewayJwtAuthorization section.</param>
        public static IApplicationBuilder UseGatewayJwtAuthorization(
            this IApplicationBuilder app,
            IConfiguration configuration)
        {
            var option = configuration.GetSection<GatewayJwtAuthorizationOptions>();

            if (option == null)
            {
                throw new InvalidOperationException(
                    String.Format(Properties.Resources.GatewayJwtAuthorizationMissingSection,
                    Helpers.GetSectionName<GatewayJwtAuthorizationOptions>()));
            }

            return app.UseMiddleware<GatewayAuthorizationMiddleware>(option);
        }

        /// <summary>
        /// Add gateway authentication middleware to request pipeline.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/> where middleware is added.</param>
        /// <param name="configureOptions">Function for obtaining <see cref="GatewayJwtAuthorizationOptions"/>.</param>
        public static IApplicationBuilder UseGatewayJwtAuthorization(
            this IApplicationBuilder app,
            Func<GatewayJwtAuthorizationOptions> configureOptions)
            => app.UseMiddleware<GatewayAuthorizationMiddleware>(configureOptions());
    }
}