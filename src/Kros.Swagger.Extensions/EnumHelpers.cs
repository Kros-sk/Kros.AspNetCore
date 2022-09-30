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
    internal static class EnumHelpers
    {
#pragma warning disable IDE1006 // Naming Styles
        internal readonly static Action<OpenApiSchema, SchemaFilterContext> DummySchemaAction = (_, __) => { };
        internal readonly static Action<OpenApiParameter, ParameterFilterContext> DummyParameterAction = (_, __) => { };
        internal readonly static IEnumerable<string> DummyXmlDocPaths = Enumerable.Empty<string>();
#pragma warning restore IDE1006 // Naming Styles

        internal static List<Type> CheckTypes(IEnumerable<Type> sourceTypes, string paramName)
        {
            List<Type> result = new List<Type>();
            foreach (Type enumType in sourceTypes)
            {
                if (!enumType.IsEnum)
                {
                    string typeName = enumType.Name;
                    string typeFullName = enumType.FullName;
                    string msg = $"{nameof(EnumSchemaFilter)} can be used only with Enum types. "
                        + $"The type {typeName} ({typeFullName}) is not an enum.";
                    throw new ArgumentException(msg, paramName);
                }
                result.Add(enumType);
            }
            return result;
        }

        internal static List<XDocument> LoadXmlDocs(IEnumerable<string> xmlDocPaths)
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

        internal static string CreateEnumMembersDescription(Type enumType, IEnumerable<XDocument> xmlDocs)
        {
            string fullTypeName = enumType.FullName;

            StringBuilder sbDescription = new StringBuilder();
            sbDescription.AppendLine("<p>Members:</p>");
            sbDescription.AppendLine("<ul>");
            foreach (string enumMemberName in Enum.GetNames(enumType))
            {
                int enumValue = (int)Enum.Parse(enumType, enumMemberName);
                string enumMemberSummary = GetXmlSummaryForEnumMember($"F:{fullTypeName}.{enumMemberName}", xmlDocs);
                sbDescription.AppendLine(CreateEnumMemberDescription(enumValue, enumMemberName, enumMemberSummary));
            }
            sbDescription.AppendLine("</ul>");
            return sbDescription.ToString();
        }

        private static string CreateEnumMemberDescription(int enumValue, string enumMemberName, string enumXmlSummary)
            => string.IsNullOrEmpty(enumXmlSummary)
                ? $"<li>{enumMemberName} ({enumValue})</li>"
                : $"<li>{enumMemberName} ({enumValue}) — {enumXmlSummary}</li>";

        private static string GetXmlSummaryForEnumMember(string enumMemberName, IEnumerable<XDocument> xmlDocs)
        {
            string enumMemberSummary = "";
            foreach (XDocument xmlDoc in xmlDocs)
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
    }
}
