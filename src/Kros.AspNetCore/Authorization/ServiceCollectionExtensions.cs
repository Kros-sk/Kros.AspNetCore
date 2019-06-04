using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Authorization extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure gateway authorization.
        /// </summary>
        /// <param name="services">Collection of app services.</param>
        public static IServiceCollection AddGatewayJwtAuthorization(this IServiceCollection services)
            => services.AddHttpClient(GatewayAuthorizationMiddleware.AuthorizationHttpClientName)
            .Services;

        /// <summary>
        /// Configure downstream api authentication.
        /// </summary>
        /// <param name="services">Collection of app services.</param>
        /// <param name="scheme">Scheme name for authentication.</param>
        /// <param name="configuration">Configuration from which the options are loaded.</param>
        /// <param name="configureOptions">Configuration.</param>
        public static IServiceCollection AddApiJwtAuthentication(
            this IServiceCollection services,
            string scheme,
            IConfiguration configuration,
            Action<JwtBearerOptions> configureOptions = null)
        {
            ApiJwtAuthorizationOptions options = configuration.GetSection<ApiJwtAuthorizationOptions>();

            services.AddAuthentication(scheme)
                .AddJwtBearer(scheme, x =>
                {
                    x.RequireHttpsMetadata = options.RequireHttpsMetadata;
                    x.SaveToken = false;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.JwtSecret)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                    configureOptions?.Invoke(x);
                });

            return services;
        }
    }
}
