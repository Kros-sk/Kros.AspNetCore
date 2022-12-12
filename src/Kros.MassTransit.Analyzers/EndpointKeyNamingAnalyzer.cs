using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kros.MassTransit.Analyzers
{
    /// <summary>
    /// Analyzer for endpoint key naming.
    /// </summary>
    /// <seealso cref="Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer" />
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EndpointKeyNamingAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The diagnostic identifier.
        /// </summary>
        public const string DiagnosticId = "KRMT001";

        /// <summary>
        /// Creates new nameproperty.
        /// </summary>
        public const string NewNameProperty = "NewName";

        private static readonly LocalizableString _title = new LocalizableResourceString(
            nameof(Resources.EndpointKeyNamingAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _message = new LocalizableResourceString(
            nameof(Resources.EndpointKeyNamingAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(
            nameof(Resources.EndpointKeyNamingAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = "Naming";
        private const string MessagePrefix = "I";
        private const string MessageSufix = "Message";
        private const string ConfigureSubscrionMethodName = "ConfigureSubscription";
        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId,
            _title,
            _message,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: _description);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            InvocationExpressionSyntax node = (InvocationExpressionSyntax)context.Node;

            if (node.Expression is MemberAccessExpressionSyntax expression
                && expression.Name is GenericNameSyntax name
                && name.Identifier.ValueText == ConfigureSubscrionMethodName)
            {
                ArgumentSyntax invocationArgument = node.ArgumentList.Arguments.FirstOrDefault();
                TypeSyntax genericType = name.TypeArgumentList.Arguments.FirstOrDefault();
                if (invocationArgument is null || genericType is null)
                {
                    return;
                }

                (string endpointKeyName, Location location) = GetEndpointKeyName(context, invocationArgument);

                if (endpointKeyName != null)
                {
                    DiagnosticReport(context, genericType, endpointKeyName, location);
                }
            }
        }

        private static (string, Location) GetEndpointKeyName(SyntaxNodeAnalysisContext context, ArgumentSyntax arg)
        {
            if (arg.Expression is MemberAccessExpressionSyntax member)
            {
                if (member.Expression is ElementAccessExpressionSyntax element)
                {
                    ArgumentSyntax argument = element.ArgumentList.Arguments.FirstOrDefault();
                    if (argument?.Expression is LiteralExpressionSyntax literal
                        && literal.Kind() == SyntaxKind.StringLiteralExpression)
                    {
                        Optional<object> value = context.SemanticModel.GetConstantValue(literal);
                        if (value.HasValue)
                        {
                            return (value.Value.ToString(), literal.GetLocation());
                        }
                    }
                }
            }

            return (null, null);
        }

        private static void DiagnosticReport(
            SyntaxNodeAnalysisContext context,
            TypeSyntax genericType,
            string endpointKeyName,
            Location location)
        {
            ITypeSymbol type = context.SemanticModel.GetTypeInfo(genericType).Type;
            string typeName = type?.Name;

            string prefix = type.TypeKind == TypeKind.Interface ? MessagePrefix : string.Empty;
            if (typeName != $"{prefix}{endpointKeyName}{MessageSufix}")
            {
                Diagnostic diagnostic = Diagnostic.Create(_rule, location, new Dictionary<string, string>()
                    { { NewNameProperty, GetNewName(type.TypeKind, typeName)} }.ToImmutableDictionary(),
                    endpointKeyName, typeName);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private static string GetNewName(TypeKind typeKind, string messageTypeName)
        {
            int offset = typeKind == TypeKind.Interface ? 1 : 0;

            return messageTypeName.Substring(offset, messageTypeName.IndexOf(MessageSufix) - offset);
        }
    }
}
