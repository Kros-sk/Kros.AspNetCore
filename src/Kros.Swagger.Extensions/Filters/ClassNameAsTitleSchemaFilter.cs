using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Kros.Swagger.Extensions.Filters
{
    internal sealed class ClassNameAsTitleSchemaFilter : ISchemaFilter
    {
        void ISchemaFilter.Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (string.IsNullOrEmpty(schema.Title)
                && context.Type != null
                && !IsPrimitive(context.Type))
            {
                schema.Title = context.Type.Name;
            }
        }

        private static bool IsPrimitive(Type type)
        {
            // String is a primitive type for these purposes.
            if (type.IsPrimitive || type == typeof(string))
            {
                return true;
            }
            Type nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                return nullableType.IsPrimitive || nullableType == typeof(string);
            }
            return false;
        }
    }
}
