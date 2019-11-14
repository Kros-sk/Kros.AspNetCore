using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
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
        private const string SwaggerDocumentationSectionName = "SwaggerDocumentation";
        private const string DefaultOAuthClientId = "kros_postman";

        /// <summary>
        /// Registers Swagger documentation generator to IoC container.
        /// </summary>
        /// <param name="services">IoC container.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="setupAction">Action for configuring swagger generating options.</param>
        public static IServiceCollection AddSwaggerDocumentation(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<SwaggerGenOptions> setupAction = null)
        {
            OpenApiInfo swaggerDocumentationSettings = GetSwaggerDocumentationSettings(configuration);

            if (swaggerDocumentationSettings is null)
            {
                throw new InvalidOperationException(
                    string.Format(Properties.Resources.SwaggerDocMissingSection, SwaggerDocumentationSectionName));
            }

            services.ConfigureSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName); // https://wegotcode.com/microsoft/swagger-fix-for-dotnetcore/
            });

            string assemblyName = AppDomain.CurrentDomain.FriendlyName;

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(swaggerDocumentationSettings.Version, swaggerDocumentationSettings);

                string documentationFilePath = GetXmlDocumentationFilePath(assemblyName);
                if (File.Exists(documentationFilePath))
                {
                    c.IncludeXmlComments(documentationFilePath);
                }

                c.DocumentFilter<EnumDocumentFilter>();
                AddSwaggerSecurity(c, swaggerDocumentationSettings);

                setupAction?.Invoke(c);
            });

            return services;
        }

        private static void AddSwaggerSecurity(SwaggerGenOptions swaggerOptions, OpenApiInfo swaggerDocumentationSettings)
        {
            if (swaggerDocumentationSettings.Extensions.TryGetValue("TokenUrl", out IOpenApiExtension t) &&
                t is OpenApiString tokenUrl)
            {
                swaggerOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Password = new OpenApiOAuthFlow()
                        {
                            TokenUrl = new Uri(tokenUrl?.Value)
                        }
                    }
                });
                swaggerOptions.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            }
        }

        private static OpenApiInfo GetSwaggerDocumentationSettings(IConfiguration configuration)
        {
            IConfigurationSection configurationSection = configuration.GetSection(SwaggerDocumentationSectionName);

            return configurationSection.Exists() ? configurationSection.Get<OpenApiInfo>() : null;
        }

        private static string GetXmlDocumentationFilePath(string assemblyName)
            => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName + ".xml");

        /// <summary>
        /// Adds Swagger documentation generator middleware.
        /// </summary>
        /// <param name="app">Application.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="setupAction">Action for configuring swagger options.</param>
        /// <param name="setupUiAction">Action for configuring swagger UI options.</param>
        public static IApplicationBuilder UseSwaggerDocumentation(
            this IApplicationBuilder app,
            IConfiguration configuration,
            Action<SwaggerOptions> setupAction = null,
            Action<SwaggerUIOptions> setupUiAction = null)
        {
            OpenApiInfo swaggerDocumentationSettings = GetSwaggerDocumentationSettings(configuration);

            if (swaggerDocumentationSettings is null)
            {
                throw new InvalidOperationException(
                    string.Format(Properties.Resources.SwaggerDocMissingSection, SwaggerDocumentationSectionName));
            }

            string clientId = GetOAuthClientId(swaggerDocumentationSettings);

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
                {
                    swaggerDoc.Servers.Add(new OpenApiServer() { Url = "/" });
                });

                setupAction?.Invoke(c);
            })

            .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1/swagger.json", swaggerDocumentationSettings.Title);
                c.OAuthClientId(clientId);
                c.OAuthClientSecret(string.Empty);

                setupUiAction?.Invoke(c);
            });

            return app;
        }

        private static string GetOAuthClientId(OpenApiInfo swaggerDocumentationSettings)
        {
            if (swaggerDocumentationSettings.Extensions.TryGetValue("OAuthClientId", out IOpenApiExtension c) &&
                c is OpenApiString clientId)
            {
                return clientId?.Value;
            }

            return DefaultOAuthClientId;
        }
    }
}
