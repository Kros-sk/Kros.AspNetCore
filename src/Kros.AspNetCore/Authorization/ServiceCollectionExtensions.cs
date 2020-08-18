using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
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
            => services
            .AddMemoryCache()
            .AddHttpClient(GatewayAuthorizationMiddleware.AuthorizationHttpClientName)
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
            return AddApiJwtAuthentication(services, new string[] { scheme }, configuration, configureOptions);
        }

        /// <summary>
        /// Configure downstream api authentication.
        /// </summary>
        /// <param name="services">Collection of app services.</param>
        /// <param name="schemeNames">Scheme names for authentication.</param>
        /// <param name="configuration">Configuration from which the options are loaded.</param>
        /// <param name="configureOptions">Configuration.</param>
        public static IServiceCollection AddApiJwtAuthentication(
            this IServiceCollection services,
            IEnumerable<string> schemeNames,
            IConfiguration configuration,
            Action<JwtBearerOptions> configureOptions = null)
        {
            ApiJwtAuthorizationOptions configuredOptionsList = configuration.GetSection<ApiJwtAuthorizationOptions>();
            IEnumerable<ApiJwtAuthorizationScheme> schemeList = (from scheme in configuredOptionsList.Schemes
                                                                 where schemeNames.Contains(scheme.SchemeName)
                                                                 select scheme);

            if (!schemeList.Any())
            {
                throw new ArgumentException("No valid schemes for Api JWT authentication", nameof(schemeNames));
            }

            AuthenticationBuilder builder;

            if (schemeList.Count() == 1)
            {
                builder = services.AddAuthentication(schemeList.First().SchemeName);
            }
            else
            {
                builder = services.AddAuthentication();
            }

            foreach (var scheme in schemeList)
            {
                builder = builder.AddJwtBearer(scheme.SchemeName, x =>
                {
                    x.RequireHttpsMetadata = scheme.RequireHttpsMetadata;
                    x.SaveToken = false;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(scheme.JwtSecret)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                    configureOptions?.Invoke(x);
                });
            }

            return services;
        }
    }
}
