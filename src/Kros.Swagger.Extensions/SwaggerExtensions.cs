using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
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
                app.UseSwagger(c => c.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
                {
                    swaggerDoc.BasePath = "/";
                }))
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("../swagger/v1/swagger.json", "Titan API V1");
                    c.OAuthClientId("kros_titan_postman");
                    c.OAuthClientSecret(string.Empty);
                });
            }

            return app;
        }
    }

    /// <summary>
    /// Adds enum value descriptions to Swagger.
    /// </summary>
    public class EnumDocumentFilter : IDocumentFilter
    {
        /// <inheritdoc />
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (KeyValuePair<string, Schema> schemaDictionaryItem in swaggerDoc.Definitions)
            {
                Schema schema = schemaDictionaryItem.Value;

                foreach (KeyValuePair<string, Schema> propertyDictionaryItem in schema.Properties)
                {
                    Schema property = propertyDictionaryItem.Value;
                    IList<object> propertyEnums = property.Enum;

                    if (propertyEnums != null && propertyEnums.Count > 0)
                    {
                        property.Description += DescribeEnum(propertyEnums);
                    }
                }
            }

            if (swaggerDoc.Paths.Count > 0)
            {
                foreach (PathItem pathItem in swaggerDoc.Paths.Values)
                {
                    DescribeEnumParameters(pathItem.Parameters);

                    var possibleParameterisedOperations = new List<Operation> { pathItem.Get, pathItem.Post, pathItem.Put };
                    possibleParameterisedOperations
                        .FindAll(x => x != null)
                        .ForEach(x => DescribeEnumParameters(x.Parameters));
                }
            }
        }

        private static void DescribeEnumParameters(IList<IParameter> parameters)
        {
            if (parameters == null)
            {
                return;
            }

            foreach (IParameter param in parameters)
            {
                if (param.Extensions.ContainsKey("enum") && (param.Extensions["enum"] is IList<object> paramEnums) &&
                    (paramEnums.Count > 0))
                {
                    param.Description += DescribeEnum(paramEnums);
                }
            }
        }

        private static string DescribeEnum(IEnumerable<object> enums)
        {
            var enumDescriptions = new List<string>();
            Type type = null;
            foreach (object enumOption in enums)
            {
                if (type == null)
                {
                    type = enumOption.GetType();
                }
                enumDescriptions.
                    Add($"{Convert.ChangeType(enumOption, type.GetEnumUnderlyingType())} = {Enum.GetName(type, enumOption)}");
            }

            return $"{Environment.NewLine}{string.Join(Environment.NewLine, enumDescriptions)}";
        }
    }
}
