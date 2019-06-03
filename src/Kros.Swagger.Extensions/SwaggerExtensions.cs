using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Swagger service extensions.
    /// </summary>
    public static class SwaggerExtensions
    {
#pragma warning disable IDE1006 // Naming Styles
        private const string SwaggerDocumentationSectionName = "SwaggerDocumentation";
        private const string DefaultOAuthClientId = "kros_postman";
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Registers Swagger documentation generator to IoC container.
        /// </summary>
        /// <param name="services">IoC container.</param>
        /// <param name="configuration">Application configuration.</param>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, IConfiguration configuration)
        {
            string assemblyName = AppDomain.CurrentDomain.FriendlyName;
            Info swaggerDocumentationSettings = GetSwaggerDocumentationSettings(configuration);

            if (swaggerDocumentationSettings != null)
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc(swaggerDocumentationSettings.Version, swaggerDocumentationSettings);
                    c.IncludeXmlComments(GetXmlDocumentationFilePath(assemblyName));
                    c.DocumentFilter<EnumDocumentFilter>();
                    if (swaggerDocumentationSettings.Extensions.TryGetValue("TokenUrl", out object t) && t is string tokenUrl)
                    {
                        c.AddSecurityDefinition("Bearer", new OAuth2Scheme
                        {
                            Type = "oauth2",
                            Flow = "password",
                            TokenUrl = tokenUrl
                        });
                        c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                        {
                        {
                            "Bearer", new string[] { }
                        }
                        });
                    }
                });
            }

            return services;
        }

        private static Info GetSwaggerDocumentationSettings(IConfiguration configuration)
        {
            IConfigurationSection configurationSection = configuration.GetSection(SwaggerDocumentationSectionName);

            return configurationSection.Exists() ? configurationSection.Get<Info>() : null;
        }

        private static string GetXmlDocumentationFilePath(string assemblyName)
            => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName + ".XML");

        /// <summary>
        /// Adds Swagger documentation generator middleware.
        /// </summary>
        /// <param name="app">Application.</param>
        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IConfiguration configuration)
        {
            Info swaggerDocumentationSettings = GetSwaggerDocumentationSettings(configuration);

            if (swaggerDocumentationSettings != null)
            {
                string clientId = GetOAuthClientId(swaggerDocumentationSettings);

                app.UseSwagger(c => c.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
                {
                    swaggerDoc.BasePath = "/";
                }))
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("../swagger/v1/swagger.json", "Titan API V1");
                    c.OAuthClientId(clientId);
                    c.OAuthClientSecret(string.Empty);
                });
            }

            return app;
        }

        private static string GetOAuthClientId(Info swaggerDocumentationSettings)
        {
            if (swaggerDocumentationSettings.Extensions.TryGetValue("OAuthClientId", out object clientId))
            {
                return clientId.ToString();
            }

            return DefaultOAuthClientId;
        }
    }
}
