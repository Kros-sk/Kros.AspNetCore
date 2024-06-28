using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Swagger service extensions.
    /// </summary>
    public static class SwaggerExtensions
    {
        private const string SwaggerDocumentationSectionName = "SwaggerDocumentation";

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
                    c.SwaggerDoc(settings.Version, settings);
                    AddAuthorization(c, settings);
                }
                if (includeXmlcomments)
                {
                    c.IncludeXmlCommentsFromAllFilesInCurrentDomainBaseDirectory();
                }
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
                    c.OAuthClientId(settings.OAuthClientId);
                    c.OAuthClientSecret(settings.OAuthClientSecret);
                    c.OAuthScopes(GetAllOAuthScopes(settings.Authorizations.Values));
                    c.OAuthUsePkce();
                }
                setupUiAction?.Invoke(c);
            });

            return app;
        }

        private static SwaggerSettings? GetSwaggerSettings(IConfiguration configuration)
        {
            IConfigurationSection configurationSection = configuration.GetSection(SwaggerDocumentationSectionName);
            return configurationSection.Exists() ? configurationSection.Get<SwaggerSettings>() : null;
        }

        private static void AddAuthorization(SwaggerGenOptions swaggerOptions, SwaggerSettings settings)
        {
            foreach (KeyValuePair<string, OpenApiSecurityScheme> auth in settings.Authorizations)
            {
                string name = auth.Key;
                OpenApiSecurityScheme scheme = auth.Value;

                swaggerOptions.AddSecurityDefinition(name, scheme);

                /// From OpenApiSecurityRequirement documentation:
                /// If the security scheme is of type "oauth2" or "openIdConnect",
                /// then the scopes value is a list of scope names required for the execution.
                /// For other security scheme types, the array MUST be empty
                List<string> scopes = [];
                if (((scheme.Type == SecuritySchemeType.OAuth2) || (scheme.Type == SecuritySchemeType.OpenIdConnect))
                    && (scheme.Flows is not null))
                {
                    GetScopes(scopes, scheme);
                    scopes = scopes.Distinct(StringComparer.Ordinal).ToList();
                }
                swaggerOptions.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = name,
                            }
                        },
                        scopes
                    }
                });
            }
        }

        private static string[] GetAllOAuthScopes(IEnumerable<OpenApiSecurityScheme> schemes)
        {
            List<string> allScopes = [];
            foreach (OpenApiSecurityScheme scheme in schemes)
            {
                GetScopes(allScopes, scheme);
            }
            return allScopes.Distinct(StringComparer.Ordinal).ToArray();
        }

        private static void GetScopes(List<string> scopes, OpenApiSecurityScheme scheme)
        {
            static void AddFlowScopes(List<string> scopes, OpenApiOAuthFlow? flow)
            {
                IEnumerable<string>? flowScopes = flow?.Scopes?.Keys;
                if (flowScopes is not null)
                {
                    scopes.AddRange(flowScopes);
                }
            }

            if (((scheme.Type == SecuritySchemeType.OAuth2) || (scheme.Type == SecuritySchemeType.OpenIdConnect))
                && (scheme.Flows is not null))
            {
                AddFlowScopes(scopes, scheme.Flows.Implicit);
                AddFlowScopes(scopes, scheme.Flows.Password);
                AddFlowScopes(scopes, scheme.Flows.ClientCredentials);
                AddFlowScopes(scopes, scheme.Flows.AuthorizationCode);
            }
        }
    }
}
