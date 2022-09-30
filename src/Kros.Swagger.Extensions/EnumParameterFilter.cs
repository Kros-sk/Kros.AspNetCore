using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Schema filter for extending enum types to support string values a data types. Enum values can be set
    /// either using integers, or enum value names.
    /// </summary>
    public sealed class EnumParameterFilter : IParameterFilter
    {
        private readonly List<Type> _enumTypes;
        private readonly List<XDocument> _xmlDocs;
        private readonly Action<OpenApiParameter, ParameterFilterContext> _parameterAction;

        /// <summary>
        /// Initialize new instance.
        /// </summary>
        /// <param name="enumTypes">Enum types to extend.</param>
        /// <param name="parameterAction">Additional schema action for manual tweaking.</param>
        /// <exception cref="ArgumentException">Some type in <paramref name="enumTypes"/> is not en Enum type.</exception>
        public EnumParameterFilter(
            IEnumerable<Type> enumTypes,
            IEnumerable<string> xmlDocPaths,
            Action<OpenApiParameter, ParameterFilterContext> parameterAction = null)
        {
            _enumTypes = EnumHelpers.CheckTypes(enumTypes, nameof(enumTypes));
            _xmlDocs = EnumHelpers.LoadXmlDocs(xmlDocPaths);
            _parameterAction = parameterAction == EnumHelpers.DummyParameterAction ? null : parameterAction;
        }

        void IParameterFilter.Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            Type paramType = context.ParameterInfo.ParameterType;
            if (_enumTypes.Contains(paramType))
            {
                string membersDescription = EnumHelpers.CreateEnumMembersDescription(paramType, _xmlDocs);
                if (!string.IsNullOrEmpty(membersDescription))
                {
                    if (string.IsNullOrEmpty(parameter.Description))
                    {
                        parameter.Description = membersDescription;
                    }
                    else
                    {
                        parameter.Description = $"<p>{parameter.Description}</p>\n{membersDescription}";
                    }
                }
                _parameterAction?.Invoke(parameter, context);
            }
        }
    }
}
