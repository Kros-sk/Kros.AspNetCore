using Kros.AspNetCore.Options;
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
        /// <exception cref="InvalidOperationException">
        /// When `GatewayJwtAuthorization` section is missing in configuration.
        /// </exception>
        public static IApplicationBuilder UseGatewayJwtAuthorization(
            this IApplicationBuilder app,
            IConfiguration configuration)
        {
            GatewayJwtAuthorizationOptions option = configuration.GetSection<GatewayJwtAuthorizationOptions>();

            if (option is null)
            {
                throw new InvalidOperationException(
                    string.Format(Properties.Resources.OptionMissingSection,
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

        /// <summary>
        /// Add <see cref="JwtBearerClaimsMiddleware"/>.
        /// For middleware dependencies registration use
        /// <see cref="ServiceCollectionExtensions.AddJwtBearerClaims(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/> where middleware is added.</param>
        public static IApplicationBuilder UseJwtBearerClaims(
            this IApplicationBuilder app)
            => app.UseMiddleware<JwtBearerClaimsMiddleware>();

        /// <summary>
        /// Add client credentials authorization middleware to request pipeline.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/> where middleware is added.</param>
        /// <param name="configuration">Configuration from which the options are loaded.
        /// Configuration must contains ClientCredentialsAuthorization section.</param>
        /// <exception cref="InvalidOperationException">
        /// When `ClientCredentialsAuthorization` section is missing in configuration.
        /// </exception>
        public static IApplicationBuilder UseClientCredentialsAuthorization(
            this IApplicationBuilder app,
            IConfiguration configuration)
        {
            ClientCredentialsAuthorizationOptions option = configuration.GetSection<ClientCredentialsAuthorizationOptions>();

            if (option is null)
            {
                throw new InvalidOperationException(
                    string.Format(Properties.Resources.OptionMissingSection,
                    Helpers.GetSectionName<ClientCredentialsAuthorizationOptions>()));
            }

            return app.UseMiddleware<ClientCredentialsAuthorizationMiddleware>(option);
        }
    }
}
