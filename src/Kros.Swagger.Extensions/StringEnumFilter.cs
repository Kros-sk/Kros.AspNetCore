using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Schema filter for extending enum types to support string values a data types. Enum values can be set
    /// either using integers, or enum value names.
    /// </summary>
    internal sealed class StringEnumFilter : ISchemaFilter, IParameterFilter
    {
#pragma warning disable IDE1006 // Naming Styles
        // This is here for Swagger, because it needs non-null values for filter descriptors, so it is able
        // to find correct constructor.
        internal readonly static Action<OpenApiSchema, SchemaFilterContext> DummySchemaAction = (_, __) => { };
        internal readonly static Action<OpenApiParameter, ParameterFilterContext> DummyParameterAction = (_, __) => { };
        internal readonly static IEnumerable<string> DummyXmlDocPaths = Enumerable.Empty<string>();
#pragma warning restore IDE1006 // Naming Styles

        private static bool FilterAllEnums(Type type) => type.IsEnum;
        private bool FilterSpecificEnums(Type type) => _enumTypes.Contains(type);

        private bool FilterCustomEnums(Type type)
        {
            bool result = _customEnumTypeFilter(type);
            if (result && !type.IsEnum)
            {
                throw new InvalidOperationException(CreateNotEnumTypeMessage(type));
            }
            return result;
        }

        private readonly List<Type> _enumTypes;
        private readonly Func<Type, bool> _customEnumTypeFilter;
        private readonly Func<Type, bool> _enumTypeFilter;
        private readonly List<XDocument> _xmlDocs;
        private readonly Action<OpenApiSchema, SchemaFilterContext> _schemaAction;
        private readonly Action<OpenApiParameter, ParameterFilterContext> _parameterAction;

        /// <summary>
        /// Creates schema and parameter filter for all enum types (types which <see cref="Type.IsEnum"/> flag is <c>true</c>).
        /// </summary>
        /// <param name="xmlDocPaths">List of paths to XML documentation files. If provided, enum members' descriptions
        /// will be read from these files.</param>
        /// <param name="schemaAction">Custom action to tweak schema documentation.</param>
        /// <param name="parameterAction">Custom action to tweak parameter documentation.</param>
        public StringEnumFilter(
            IEnumerable<string> xmlDocPaths,
            Action<OpenApiSchema, SchemaFilterContext> schemaAction = null,
            Action<OpenApiParameter, ParameterFilterContext> parameterAction = null)
        {
            _enumTypeFilter = FilterAllEnums;
            _xmlDocs = LoadXmlDocs(xmlDocPaths);
            _schemaAction = schemaAction;
            _parameterAction = parameterAction;
        }

        /// <summary>
        /// Creates schema and parameter filter for all enum types which pass <paramref name="enumTypeFilter"/>.
        /// It is always checked if type is an enum, even if this user filter function returns <c>true</c>.
        /// </summary>
        /// <param name="enumTypeFilter">Filter, which decides, which enum types will be documented with string values.</param>
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
        /// <exception cref="ArgumentNullException">The value of <paramref name="enumTypeFilter"/> is <c>null</c>.</exception>
        public StringEnumFilter(
            Func<Type, bool> enumTypeFilter,
            IEnumerable<string> xmlDocPaths,
            Action<OpenApiSchema, SchemaFilterContext> schemaAction = null,
            Action<OpenApiParameter, ParameterFilterContext> parameterAction = null)
        {
            if (enumTypeFilter is null)
            {
                throw new ArgumentNullException(nameof(enumTypeFilter));
            }
            _customEnumTypeFilter = enumTypeFilter;
            _enumTypeFilter = FilterCustomEnums;
            _xmlDocs = LoadXmlDocs(xmlDocPaths);
            _schemaAction = schemaAction;
            _parameterAction = parameterAction;
        }

        /// <summary>
        /// Creates schema and parameter filter for enum types specified in <paramref name="enumTypes"/>.
        /// </summary>
        /// <param name="enumTypes">List of enum types which will be documented with string values.</param>
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
        /// <exception cref="ArgumentException">
        /// Some type in <paramref name="enumTypes"/> list is not an <c>Enum</c> type.
        /// </exception>
        public StringEnumFilter(
            IEnumerable<Type> enumTypes,
            IEnumerable<string> xmlDocPaths,
            Action<OpenApiSchema, SchemaFilterContext> schemaAction = null,
            Action<OpenApiParameter, ParameterFilterContext> parameterAction = null)
        {
            _enumTypes = CheckTypes(enumTypes, nameof(enumTypes));
            _enumTypeFilter = FilterSpecificEnums;
            _xmlDocs = LoadXmlDocs(xmlDocPaths);
            _schemaAction = schemaAction;
            _parameterAction = parameterAction;
        }

        /// <inheritdoc/>
        void ISchemaFilter.Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (_enumTypeFilter(context.Type))
            {
                schema.Type = "string | integer";
                schema.Format = Enum.GetUnderlyingType(context.Type).Name;
                schema.Description = CreateEnumMembersDescription(context.Type, schema.Description);
                schema.Enum = CreateStringEnumMembers(context.Type);
                _schemaAction?.Invoke(schema, context);
            }
        }

        /// <inheritdoc/>
        void IParameterFilter.Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            Type paramType = context.ParameterInfo.ParameterType;
            if (_enumTypeFilter(paramType))
            {
                parameter.Description = CreateEnumMembersDescription(paramType, parameter.Description);
                _parameterAction?.Invoke(parameter, context);
            }
        }

        private static List<IOpenApiAny> CreateStringEnumMembers(Type enumType)
            => new List<IOpenApiAny>(Enum.GetNames(enumType).Select(enumMemberName => new OpenApiString(enumMemberName)));

        private static List<Type> CheckTypes(IEnumerable<Type> sourceTypes, string paramName)
        {
            List<Type> result = new List<Type>();
            foreach (Type enumType in sourceTypes)
            {
                if (!enumType.IsEnum)
                {
                    throw new ArgumentException(CreateNotEnumTypeMessage(enumType), paramName);
                }
                result.Add(enumType);
            }
            return result;
        }

        private static string CreateNotEnumTypeMessage(Type type)
        {
            string typeName = type.Name;
            string typeFullName = type.FullName;
            return $"{nameof(StringEnumFilter)} can be used only with Enum types. "
                + $"The type {typeName} ({typeFullName}) is not an enum.";
        }

        private static List<XDocument> LoadXmlDocs(IEnumerable<string> xmlDocPaths)
        {
            List<XDocument> result = new List<XDocument>();
            if (xmlDocPaths != null)
            {
                foreach (string xmlDocPath in xmlDocPaths)
                {
                    if (File.Exists(xmlDocPath))
                    {
                        result.Add(XDocument.Load(xmlDocPath));
                    }
                }
            }
            return result;
        }

        private string CreateEnumMembersDescription(Type enumType, string defaultDescription)
        {
            StringBuilder sb = new StringBuilder();
            AppendDefaultDescription(sb, defaultDescription);
            AppendEnumMembers(sb, enumType);
            return sb.ToString();
        }

        private static void AppendDefaultDescription(StringBuilder sb, string defaultDescription)
        {
            if (!string.IsNullOrEmpty(defaultDescription))
            {
                sb.Append("<p>");
                sb.Append(defaultDescription);
                sb.AppendLine("</p>");
            }
        }

        private void AppendEnumMembers(StringBuilder sb, Type enumType)
        {
            sb.AppendLine("<p>Members:</p>");
            sb.AppendLine("<ul>");
            foreach (string enumMemberName in Enum.GetNames(enumType))
            {
                // This may lead to errors (runtime or maybe just in documentation), if underlying type will be unsigned.
                long enumValue = Convert.ToInt64(Enum.Parse(enumType, enumMemberName));
                string enumMemberSummary = GetXmlSummaryForEnumMember($"F:{enumType.FullName}.{enumMemberName}");
                sb.AppendLine(CreateEnumMemberDescription(enumValue, enumMemberName, enumMemberSummary));
            }
            sb.AppendLine("</ul>");
        }

        private string GetXmlSummaryForEnumMember(string enumMemberName)
        {
            string enumMemberSummary = "";
            foreach (XDocument xmlDoc in _xmlDocs)
            {
                XElement enumMemberComments = xmlDoc.Descendants("member")
                    .FirstOrDefault(m => m.Attribute("name").Value.Equals(enumMemberName, StringComparison.OrdinalIgnoreCase));
                if (enumMemberComments == null)
                {
                    continue;
                }
                XElement summary = enumMemberComments.Descendants("summary").FirstOrDefault();
                if (summary == null)
                {
                    continue;
                }
                else
                {
                    enumMemberSummary = summary.Value.Trim();
                }
                if (!string.IsNullOrEmpty(enumMemberSummary))
                {
                    break;
                }
            }
            return enumMemberSummary;
        }

        private static string CreateEnumMemberDescription(long enumValue, string enumMemberName, string enumXmlSummary)
            => string.IsNullOrEmpty(enumXmlSummary)
                ? $"<li>{enumMemberName} ({enumValue})</li>"
                : $"<li>{enumMemberName} ({enumValue}) — {enumXmlSummary}</li>";
    }
}
