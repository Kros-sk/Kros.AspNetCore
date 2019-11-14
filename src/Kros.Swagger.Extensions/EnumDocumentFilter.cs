using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Adds enum value descriptions to Swagger.
    /// </summary>
    public class EnumDocumentFilter : IDocumentFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (OpenApiSchema schema in swaggerDoc.Components.Schemas.Values)
            {
                foreach (OpenApiSchema property in schema.Properties.Values)
                {
                    IList<IOpenApiAny> propertyEnums = property.Enum;

                    if (propertyEnums != null && propertyEnums.Count > 0)
                    {
                        property.Description += DescribeEnum(propertyEnums);
                    }
                }
            }

            if (swaggerDoc.Paths.Count > 0)
            {
                foreach (OpenApiPathItem pathItem in swaggerDoc.Paths.Values)
                {
                    DescribeEnumParameters(pathItem.Parameters);

                    pathItem.Operations.TryGetValue(OperationType.Get, out OpenApiOperation getOp);
                    pathItem.Operations.TryGetValue(OperationType.Post, out OpenApiOperation postOp);
                    pathItem.Operations.TryGetValue(OperationType.Put, out OpenApiOperation putOp);

                    var possibleParameterisedOperations = new List<OpenApiOperation> { getOp, postOp, putOp };
                    possibleParameterisedOperations
                        .FindAll(x => x != null)
                        .ForEach(x => DescribeEnumParameters(x.Parameters));
                }
            }
        }

        private static void DescribeEnumParameters(IList<OpenApiParameter> parameters)
        {
            if (parameters is null)
            {
                return;
            }

            foreach (OpenApiParameter param in parameters)
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
