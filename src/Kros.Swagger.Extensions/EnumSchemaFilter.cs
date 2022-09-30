using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Schema filter for extending enum types to support string values a data types. Enum values can be set
    /// either using integers, or enum value names.
    /// </summary>
    public sealed class EnumSchemaFilter : ISchemaFilter
    {
        private readonly List<Type> _enumTypes;
        private readonly List<XDocument> _xmlDocs;
        private readonly Action<OpenApiSchema, SchemaFilterContext> _schemaAction;

        /// <summary>
        /// Initialize new instance.
        /// </summary>
        /// <param name="enumTypes">Enum types to extend.</param>
        /// <param name="schemaAction">Additional schema action for manual tweaking.</param>
        /// <exception cref="ArgumentException">Some type in <paramref name="enumTypes"/> is not en Enum type.</exception>
        public EnumSchemaFilter(
            IEnumerable<Type> enumTypes,
            IEnumerable<string> xmlDocPaths,
            Action<OpenApiSchema, SchemaFilterContext> schemaAction = null)
        {
            _enumTypes = EnumHelpers.CheckTypes(enumTypes, nameof(enumTypes));
            _xmlDocs = EnumHelpers.LoadXmlDocs(xmlDocPaths);
            _schemaAction = schemaAction == EnumHelpers.DummySchemaAction ? null : schemaAction;
        }

        /// <inheritdoc/>
        void ISchemaFilter.Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (_enumTypes.Contains(context.Type))
            {
                schema.Type = "string | integer";
                schema.Format = Enum.GetUnderlyingType(context.Type).Name;
                schema.Description = EnumHelpers.CreateEnumMembersDescription(context.Type, _xmlDocs);
                schema.Enum = CreateStringEnumMembers(context.Type);
                _schemaAction?.Invoke(schema, context);
            }
        }

        private static List<IOpenApiAny> CreateStringEnumMembers(Type enumType)
            => new List<IOpenApiAny>(Enum.GetNames(enumType).Select(enumMemberName => new OpenApiString(enumMemberName)));
    }
}
