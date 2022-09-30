using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Extensions for <see cref="SwaggerGenOptions "/> class.
    /// </summary>
    public static class SwaggerGenOptionsExtensions
    {
        /// <summary>
        /// Add <see cref="EnumSchemaFilter"/> to Swagger generator options.
        /// </summary>
        /// <param name="opt">Swagger generator options.</param>
        /// <param name="enumTypes">Enum types to extend. All types must be of Enum type, or <see cref="ArgumentException"/>
        /// will be thrown.</param>
        /// <param name="schemaAction">Additional schema action for manual tweaking.</param>
        /// <returns>Input <paramref name="opt"/> value for fluent chaining.</returns>
        public static SwaggerGenOptions AddStringEnumSchemaFilter(
            this SwaggerGenOptions opt,
            IEnumerable<Type> enumTypes,
            IEnumerable<string> xmlDocPaths = null,
            Action<OpenApiSchema, SchemaFilterContext> schemaAction = null,
            Action<OpenApiParameter, ParameterFilterContext> parameterAction = null)
        {
            opt.SchemaFilter<EnumSchemaFilter>(
                enumTypes,
                xmlDocPaths ?? EnumHelpers.DummyXmlDocPaths,
                schemaAction ?? EnumHelpers.DummySchemaAction);
            opt.ParameterFilter<EnumParameterFilter>(
                enumTypes,
                xmlDocPaths ?? EnumHelpers.DummyXmlDocPaths,
                parameterAction ?? EnumHelpers.DummyParameterAction);
            return opt;
        }

        /// <summary>
        /// Include all XML comment files from folder <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="opt">Swagger generator options.</param>
        /// <param name="folderPath">Folder where to look for <c>.xml</c> files.</param>
        /// <param name="filePathFilter">Function to test if file will be included. If not set, all <c>.xml</c> files in
        /// <paramref name="folderPath"/> folder will be included.</param>
        /// <returns>Input <paramref name="opt"/> value for fluent chaining.</returns>
        public static SwaggerGenOptions IncludeXmlCommentsFromAllXmlFiles(
            this SwaggerGenOptions opt,
            string folderPath,
            Func<string, bool> filePathFilter = null)
        {
            foreach (string filePath in Directory.EnumerateFiles(folderPath, "*.xml"))
            {
                bool addFile = filePathFilter is null || filePathFilter(filePath);
                if (addFile)
                {
                    opt.IncludeXmlComments(filePath);
                }
            }
            return opt;
        }

        internal static SwaggerGenOptions IncludeXmlCommentsFromCurrentDomainAssembly(this SwaggerGenOptions opt)
        {
            string assemblyName = AppDomain.CurrentDomain.FriendlyName;
            string documentationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName + ".xml");
            if (File.Exists(documentationFilePath))
            {
                opt.IncludeXmlComments(documentationFilePath);
            }
            return opt;
        }
    }
}
