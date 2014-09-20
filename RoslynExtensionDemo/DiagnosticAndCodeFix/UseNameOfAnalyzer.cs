using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DiagnosticAnalyzerAndCodeFix
{
    [DiagnosticAnalyzer, ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    public class UseNameOfAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string DiagnosticId = "UseNameOf";
        internal const string Description = "Use nameof";
        internal const string MessageFormat = "String literals are easy to get wrong. Consider using the nameof() operator instead.";
        internal const string Category = "Language";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest => ImmutableArray.Create(SyntaxKind.StringLiteralExpression);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            if ((node.SyntaxTree.Options as CSharpParseOptions)?.LanguageVersion < LanguageVersion.CSharp6)
                return;
            
            var literal = ((LiteralExpressionSyntax)node).Token.ValueText;

            var container = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (container == null)
                return;

            var containerSymbol = semanticModel.GetDeclaredSymbol(container);
            if (containerSymbol.Name != literal)
                return;

            addDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
        }
    }
}