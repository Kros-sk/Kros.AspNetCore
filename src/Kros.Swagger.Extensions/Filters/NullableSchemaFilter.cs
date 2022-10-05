using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Reflection;

namespace Kros.Swagger.Extensions.Filters
{
    internal sealed class NullableSchemaFilter : ISchemaFilter
    {
        void ISchemaFilter.Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.MemberInfo != null)
            {
                Type? memberType = GetMemberType(context.MemberInfo);
                if (memberType != null)
                {
                    if (memberType.IsValueType)
                    {
                        schema.Nullable = Nullable.GetUnderlyingType(memberType) != null;
                    }
                    else
                    {
                        schema.Nullable = !context.MemberInfo.IsNonNullableReferenceType();
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
