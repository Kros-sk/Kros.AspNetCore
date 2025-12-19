using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Reflection;

namespace Kros.Swagger.Extensions.Filters
{
    internal sealed class NullableSchemaFilter : ISchemaFilter
    {
        void ISchemaFilter.Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema is OpenApiSchema openApiSchema && context.MemberInfo != null)
            {
                Type? memberType = GetMemberType(context.MemberInfo);
                if (memberType != null)
                {
                    bool isNullable = memberType.IsValueType
                        ? Nullable.GetUnderlyingType(memberType) != null
                        : !context.MemberInfo.IsNonNullableReferenceType();

                    if (isNullable && openApiSchema.Type.HasValue)
                    {
                        openApiSchema.Type |= JsonSchemaType.Null;
                    }
                }
            }
        }

        private static Type? GetMemberType(MemberInfo memberInfo)
            => memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
                MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
                _ => null
            };
    }
}
