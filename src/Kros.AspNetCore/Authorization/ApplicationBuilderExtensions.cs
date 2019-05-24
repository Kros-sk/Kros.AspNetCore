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
        /// <param name="app">IApplicationBuilder where middleware is added.</param>
        /// <param name="configuration">Configuration from which the options are loaded.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseGatewayJwtAuthorization(
            this IApplicationBuilder app,
            IConfiguration configuration)
            => app.UseMiddleware<GatewayAuthorizationMiddleware>(configuration.GetSection<GatewayJwtAuthorizationOptions>());

        /// <summary>
        /// Add gateway authentication middleware to request pipeline.
        /// </summary>
        /// <param name="app">IApplicationBuilder where middleware is added.</param>
        /// <param name="configureOptions">Func returning <see cref="GatewayJwtAuthorizationOptions"/></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGatewayJwtAuthorization(
            this IApplicationBuilder app,
            Func<GatewayJwtAuthorizationOptions> configureOptions)
            => app.UseMiddleware<GatewayAuthorizationMiddleware>(configureOptions.Invoke());
    }
}