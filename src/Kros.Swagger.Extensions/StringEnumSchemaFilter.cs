using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Schema filter for extending enum types to support string values a data types. Enum values can be set
    /// either using integers, or enum value names.
    /// </summary>
    public sealed class StringEnumSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// Dummy schema action. Swagger is not able to find correct constructor, if tha value of constructor's
        /// parameter is <c>null</c>. So we need to pass correct typed value.
        /// </summary>
        internal static Action<OpenApiSchema, SchemaFilterContext> DummySchemaAction { get; } = (_, __) => { };

        private readonly List<Type> _enumTypes = new List<Type>();
        private readonly Action<OpenApiSchema, SchemaFilterContext> _schemaAction;

        /// <summary>
        /// Initialize new instance.
        /// </summary>
        /// <param name="enumTypes">Enum types to extend.</param>
        /// <param name="schemaAction">Additional schema action for manual tweaking.</param>
        /// <exception cref="ArgumentException">Some type in <paramref name="enumTypes"/> is not en Enum type.</exception>
        public StringEnumSchemaFilter(
            IEnumerable<Type> enumTypes,
            Action<OpenApiSchema, SchemaFilterContext> schemaAction = null)
        {
            foreach (Type enumType in enumTypes)
            {
                if (!enumType.IsEnum)
                {
                    string typeName = enumType.Name;
                    string typeFullName = enumType.FullName;
                    string msg = $"{nameof(StringEnumSchemaFilter)} can be used only with Enum types. "
                        + $"The type {typeName} ({typeFullName}) is not an enum.";
                    throw new ArgumentException(msg, nameof(enumTypes));
                }
                _enumTypes.Add(enumType);
            }
            if (schemaAction == DummySchemaAction)
            {
                schemaAction = null;
            }
            _schemaAction = schemaAction;
        }

        /// <inheritdoc/>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (_enumTypes.Contains(context.Type))
            {
                schema.Type = "string | integer";
                schema.Format = Enum.GetUnderlyingType(context.Type).Name;
                schema.Enum.Clear();
                foreach (string enumName in Enum.GetNames(context.Type))
                {
                    int enumValue = (int)Enum.Parse(context.Type, enumName);
                    schema.Enum.Add(new OpenApiString($"\"{enumName}\" ({enumValue})", true));
                }
                _schemaAction?.Invoke(schema, context);
            }
        }
    }
}
