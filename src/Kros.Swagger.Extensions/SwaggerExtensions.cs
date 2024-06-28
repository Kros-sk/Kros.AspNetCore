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
        /// <param name="includeXmlcomments">
        /// If <c>true</c>, includes XML comments from all <c>.xml</c> files in current domain base directory
        /// (<see cref="SwaggerGenOptionsExtensions.IncludeXmlCommentsFromAllFilesInCurrentDomainBaseDirectory(SwaggerGenOptions)"/>).
        /// </param>
        /// <param name="setupAction">Action for configuring swagger generating options.</param>
        public static IServiceCollection AddSwaggerDocumentation(
            this IServiceCollection services,
            IConfiguration configuration,
            bool includeXmlcomments,
            Action<SwaggerGenOptions>? setupAction = null)
        {
            SwaggerSettings? settings = GetSwaggerSettings(configuration);

            services.ConfigureSwaggerGen(options =>
            {
                // '+' (nested classes) is not valid character in OpenApi reference.
                options.CustomSchemaIds(x => x.FullName?.Replace("+", "-"));
            });

            services.AddSwaggerGen(c =>
            {
                if (settings is not null)
                {
                    c.SwaggerDoc(settings.Version, MapSwaggerSettingsToOpenApiInfo(settings));
                }
                if (includeXmlcomments)
                {
                    c.IncludeXmlCommentsFromAllFilesInCurrentDomainBaseDirectory();
                }
                //AddSwaggerSecurity(c, options);
                c.UseClassNameAsTitle();
                c.UseNullableSchemaFilter();
                setupAction?.Invoke(c);
            });

            return services;
        }

        /// <summary>
        /// Registers Swagger documentation generator to IoC container. Does not add XML documentation files.
        /// </summary>
        /// <param name="services">IoC container.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="setupAction">Action for configuring swagger generating options.</param>
        public static IServiceCollection AddSwaggerDocumentation(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<SwaggerGenOptions>? setupAction = null)
            => AddSwaggerDocumentation(services, configuration, false, setupAction);

        private static void AddSwaggerSecurity(SwaggerGenOptions swaggerOptions, OpenApiInfo swaggerDocumentationSettings)
        {
            if (swaggerDocumentationSettings.Extensions.TryGetValue("TokenUrl", out IOpenApiExtension? t) &&
                t is OpenApiString tokenUrl)
            {
                swaggerOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Password = new OpenApiOAuthFlow()
                        {
                            TokenUrl = new Uri(tokenUrl.Value)
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

        private static SwaggerSettings? GetSwaggerSettings(IConfiguration configuration)
        {
            IConfigurationSection configurationSection = configuration.GetSection(SwaggerDocumentationSectionName);
            return configurationSection.Exists() ? configurationSection.Get<SwaggerSettings>() : null;
        }

        private static OpenApiInfo? MapSwaggerSettingsToOpenApiInfo(SwaggerSettings? settings)
        {
            if (settings is null)
            {
                return null;
            }
            OpenApiInfo info = new()
            {
                Title = settings.Title,
                Description = settings.Description,
                Version = settings.Version
            };
            if (settings.Contact is not null)
            {
                info.Contact = new()
                {
                    Name = settings.Contact.Name,
                    Url = settings.Contact.Url is not null ? new Uri(settings.Contact.Url) : null,
                    Email = settings.Contact.Email
                };
            }
            if (settings.License is not null)
            {
                info.License = new()
                {
                    Name = settings.License.Name,
                    Url = settings.License.Url is not null ? new Uri(settings.License.Url) : null
                };
            }
            return info;
        }

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
            Action<SwaggerOptions>? setupAction = null,
            Action<SwaggerUIOptions>? setupUiAction = null)
        {
            SwaggerSettings? settings = GetSwaggerSettings(configuration);

            app.UseSwagger(c =>
            {
                setupAction?.Invoke(c);
            })

            .UseSwaggerUI(c =>
            {
                if (settings is not null)
                {
                    c.SwaggerEndpoint($"{settings.Version}/swagger.json", settings.Title);
                }
                setupUiAction?.Invoke(c);
            });

            return app;
        }
    }
}
