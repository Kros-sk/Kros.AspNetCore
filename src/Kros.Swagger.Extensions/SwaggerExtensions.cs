using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
#pragma warning disable IDE1006 // Naming Styles
        private const string SwaggerDocumentationSectionName = "SwaggerDocumentation";
        private const string DefaultOAuthClientId = "kros_postman";
#pragma warning restore IDE1006 // Naming Styles

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
            services.ConfigureSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName); // https://wegotcode.com/microsoft/swagger-fix-for-dotnetcore/
            });

            string assemblyName = AppDomain.CurrentDomain.FriendlyName;
            Info swaggerDocumentationSettings = GetSwaggerDocumentationSettings(configuration);

            if (swaggerDocumentationSettings != null)
            {
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
            }

            return services;
        }

        private static void AddSwaggerSecurity(SwaggerGenOptions swaggerOptions, Info swaggerDocumentationSettings)
        {
            if (swaggerDocumentationSettings.Extensions.TryGetValue("TokenUrl", out object t) && t is string tokenUrl)
            {
                swaggerOptions.AddSecurityDefinition("Bearer", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "password",
                    TokenUrl = tokenUrl
                });
                swaggerOptions.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {
                        "Bearer", new string[] { }
                    }
                });
            }
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
        /// <param name="configuration">Application configuration.</param>
        /// <param name="setupAction">Action for configuring swagger options.</param>
        /// <param name="setupUiAction">Action for configuring swagger UI options.</param>
        public static IApplicationBuilder UseSwaggerDocumentation(
            this IApplicationBuilder app,
            IConfiguration configuration,
            Action<SwaggerOptions> setupAction = null,
            Action<SwaggerUIOptions> setupUiAction = null)
        {
            Info swaggerDocumentationSettings = GetSwaggerDocumentationSettings(configuration);

            if (swaggerDocumentationSettings != null)
            {
                string clientId = GetOAuthClientId(swaggerDocumentationSettings);

                app.UseSwagger(c => c.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
                {
                    swaggerDoc.BasePath = "/";

                    setupAction?.Invoke(c);
                }))
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("../swagger/v1/swagger.json", swaggerDocumentationSettings.Title);
                    c.OAuthClientId(clientId);
                    c.OAuthClientSecret(string.Empty);

                    setupUiAction?.Invoke(c);
                });
            }

            return app;
        }

        private static string GetOAuthClientId(Info swaggerDocumentationSettings)
        {
            if (swaggerDocumentationSettings.Extensions.TryGetValue("OAuthClientId", out object c) && c is string clientId)
            {
                return clientId.ToString();
            }

            return DefaultOAuthClientId;
        }
    }
}
