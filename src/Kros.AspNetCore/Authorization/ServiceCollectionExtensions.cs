using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
        /// <returns></returns>
        public static IServiceCollection AddGatewayJwtAuthorization(this IServiceCollection services)
        {
            services.AddHttpClient(GatewayAuthorizationMiddleware.AuthorizationHttpClientName);
            return services;
        }

        /// <summary>
        /// Configure api authentication.
        /// </summary>
        /// <param name="services">Collection of app services.</param>
        /// <param name="scheme">Scheme name for authentication.</param>
        /// <param name="configuration">Configuration from which the options are loaded.</param>
        /// <returns></returns>
        public static IServiceCollection AddApiJwtAuthentication(
            this IServiceCollection services,
            string scheme,
            IConfiguration configuration)
        {
            var options = configuration.GetSection<ApiJwtAuthorizationOptions>();

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
                });

            return services;
        }

        /// <summary>
        /// Configure api authorization.
        /// </summary>
        /// <param name="services">Collection of app services.</param>
        /// <param name="scheme">Scheme name for authentication.</param>
        /// <returns></returns>
        public static IServiceCollection AddApiJwtAuthorization(
            this IServiceCollection services,
            string scheme)
        {
            return services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtAuthorizationHelper.AuthPolicyName, policyAdmin =>
                {
                    policyAdmin.AuthenticationSchemes.Add(scheme);
                    policyAdmin.RequireClaim(UserClaimTypes.IsAdmin, bool.TrueString);
                });
            });
        }

        /// <summary>
        /// Configure authentication for authorization service.
        /// </summary>
        /// <param name="services">Collection of app services.</param>
        /// <param name="scheme">Scheme name for authentication.</param>
        /// <param name="configuration">App configurations.</param>
        /// <returns></returns>
        public static IServiceCollection AddAuthJwtAuthentication(
            this IServiceCollection services,
            string scheme,
            IConfiguration configuration)
        {
            AddApiJwtAuthentication(services, scheme, configuration);
            return AddApiJwtAuthorization(services, scheme);
        }
    }
}
