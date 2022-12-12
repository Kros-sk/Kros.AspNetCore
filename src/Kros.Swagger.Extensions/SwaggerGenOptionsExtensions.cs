using Kros.Swagger.Extensions.Filters;
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
        /// Sets correct <c>nullable</c> flag in schema for nullable reference types.
        /// </summary>
        /// <param name="opt">Swagger generator options.</param>
        /// <returns>Input <paramref name="opt"/> value for fluent chaining.</returns>
        public static SwaggerGenOptions UseNullableSchemaFilter(this SwaggerGenOptions opt)
        {
            opt.SchemaFilter<NullableSchemaFilter>();
            return opt;
        }

        /// <summary>
        /// Add schema filter which sets class name to schema title. This works for reference types only.
        /// </summary>
        /// <param name="opt">Swagger generator options.</param>
        /// <returns>Input <paramref name="opt"/> value for fluent chaining.</returns>
        public static SwaggerGenOptions UseClassNameAsTitle(this SwaggerGenOptions opt)
        {
            opt.SchemaFilter<ClassNameAsTitleSchemaFilter>();
            return opt;
        }

        /// <summary>
        /// Document all enum types with string enum values.
        /// </summary>
        /// <param name="opt">Swagger generator options.</param>
        /// <param name="xmlDocPaths">
        /// <inheritdoc
        ///     cref="StringEnumFilter(IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///     path="//param[@name='xmlDocPaths']"/>
        /// </param>
        /// <param name="schemaAction">
        /// <inheritdoc
        ///     cref="StringEnumFilter(IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///     path="//param[@name='schemaAction']"/>
        /// </param>
        /// <param name="parameterAction">
        /// <inheritdoc
        ///     cref="StringEnumFilter(IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///     path="//param[@name='parameterAction']"/>
        /// </param>
        /// <returns>Input <paramref name="opt"/> value for fluent chaining.</returns>
        public static SwaggerGenOptions DocumentAllEnumsWithStrings(
            this SwaggerGenOptions opt,
            IEnumerable<string>? xmlDocPaths = null,
            Action<OpenApiSchema, SchemaFilterContext>? schemaAction = null,
            Action<OpenApiParameter, ParameterFilterContext>? parameterAction = null)
        {
            opt.SchemaFilter<StringEnumFilter>(
                xmlDocPaths ?? StringEnumFilter.DummyXmlDocPaths,
                schemaAction ?? StringEnumFilter.DummySchemaAction,
                parameterAction ?? StringEnumFilter.DummyParameterAction);

            opt.ParameterFilter<StringEnumFilter>(
                xmlDocPaths ?? StringEnumFilter.DummyXmlDocPaths,
                schemaAction ?? StringEnumFilter.DummySchemaAction,
                parameterAction ?? StringEnumFilter.DummyParameterAction);

            return opt;
        }

        /// <summary>
        /// Document enum types filtered by <paramref name="enumTypeFilter"/> with string values.
        /// </summary>
        /// <param name="opt">
        ///     <inheritdoc
        ///         cref="DocumentAllEnumsWithStrings(SwaggerGenOptions, IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///         path="//param[@name='opt']"/>
        /// </param>
        /// <param name="enumTypeFilter">Function, which filters which types will be documented with string values.
        /// The types still must be enum types.</param>
        /// <param name="xmlDocPaths">
        ///     <inheritdoc
        ///         cref="StringEnumFilter(IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///         path="//param[@name='xmlDocPaths']"/>
        /// </param>
        /// <param name="schemaAction">
        ///     <inheritdoc
        ///         cref="StringEnumFilter(IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///         path="//param[@name='schemaAction']"/>
        /// </param>
        /// <param name="parameterAction">
        ///     <inheritdoc
        ///         cref="StringEnumFilter(IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///         path="//param[@name='parameterAction']"/>
        /// </param>
        /// <returns>
        ///     <inheritdoc
        ///         cref="DocumentAllEnumsWithStrings(SwaggerGenOptions, IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///         path="//returns"/>
        /// </returns>
        public static SwaggerGenOptions DocumentEnumsWithStrings(
            this SwaggerGenOptions opt,
            Func<Type, bool> enumTypeFilter,
            IEnumerable<string>? xmlDocPaths = null,
            Action<OpenApiSchema, SchemaFilterContext>? schemaAction = null,
            Action<OpenApiParameter, ParameterFilterContext>? parameterAction = null)
        {
            opt.SchemaFilter<StringEnumFilter>(
                enumTypeFilter,
                xmlDocPaths ?? StringEnumFilter.DummyXmlDocPaths,
                schemaAction ?? StringEnumFilter.DummySchemaAction,
                parameterAction ?? StringEnumFilter.DummyParameterAction);

            opt.ParameterFilter<StringEnumFilter>(
                enumTypeFilter,
                xmlDocPaths ?? StringEnumFilter.DummyXmlDocPaths,
                schemaAction ?? StringEnumFilter.DummySchemaAction,
                parameterAction ?? StringEnumFilter.DummyParameterAction);

            return opt;
        }

        /// <summary>
        /// Document specified enum types (<paramref name="enumTypes"/>) with string values.
        /// </summary>
        /// <param name="opt">
        /// <inheritdoc
        ///     cref="DocumentAllEnumsWithStrings(SwaggerGenOptions, IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///     path="//param[@name='opt']"/>
        /// </param>
        /// <param name="enumTypes">List of enum types, which will be documented with string values.</param>
        /// <param name="xmlDocPaths">
        /// <inheritdoc
        ///     cref="StringEnumFilter(IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///     path="//param[@name='xmlDocPaths']"/>
        /// </param>
        /// <param name="schemaAction">
        /// <inheritdoc
        ///     cref="StringEnumFilter(IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///     path="//param[@name='schemaAction']"/>
        /// </param>
        /// <param name="parameterAction">
        /// <inheritdoc
        ///     cref="StringEnumFilter(IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///     path="//param[@name='parameterAction']"/>
        /// </param>
        /// <returns>
        ///     <inheritdoc
        ///         cref="DocumentAllEnumsWithStrings(SwaggerGenOptions, IEnumerable{string}, Action{OpenApiSchema, SchemaFilterContext}, Action{OpenApiParameter, ParameterFilterContext})"
        ///         path="//returns"/>
        /// </returns>
        public static SwaggerGenOptions DocumentEnumsWithStrings(
            this SwaggerGenOptions opt,
            IEnumerable<Type> enumTypes,
            IEnumerable<string>? xmlDocPaths = null,
            Action<OpenApiSchema, SchemaFilterContext>? schemaAction = null,
            Action<OpenApiParameter, ParameterFilterContext>? parameterAction = null)
        {
            opt.SchemaFilter<StringEnumFilter>(
                enumTypes,
                xmlDocPaths ?? StringEnumFilter.DummyXmlDocPaths,
                schemaAction ?? StringEnumFilter.DummySchemaAction,
                parameterAction ?? StringEnumFilter.DummyParameterAction);

            opt.ParameterFilter<StringEnumFilter>(
                enumTypes,
                xmlDocPaths ?? StringEnumFilter.DummyXmlDocPaths,
                schemaAction ?? StringEnumFilter.DummySchemaAction,
                parameterAction ?? StringEnumFilter.DummyParameterAction);

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
        public static List<string> IncludeXmlCommentsFromAllFiles(
            this SwaggerGenOptions opt,
            string folderPath,
            Func<string, bool>? filePathFilter = null)
        {
            List<string> usedXmlFiles = new();
            foreach (string filePath in Directory.EnumerateFiles(folderPath, "*.xml"))
            {
                bool addFile = filePathFilter is null || filePathFilter(filePath);
                if (addFile)
                {
                    opt.IncludeXmlComments(filePath);
                    usedXmlFiles.Add(filePath);
                }
            }
            return usedXmlFiles;
        }

        /// <summary>
        /// Include all XML comment files from current domain base folder: <c>AppDomain.CurrentDomain.BaseDirectory</c>
        /// (<see cref="AppDomain.BaseDirectory"/>).
        /// </summary>
        /// <param name="opt">Swagger generator options.</param>
        /// <returns>Input <paramref name="opt"/> value for fluent chaining.</returns>
        public static List<string> IncludeXmlCommentsFromAllFilesInCurrentDomainBaseDirectory(
            this SwaggerGenOptions opt)
            => IncludeXmlCommentsFromAllFiles(opt, AppDomain.CurrentDomain.BaseDirectory);
    }
}
