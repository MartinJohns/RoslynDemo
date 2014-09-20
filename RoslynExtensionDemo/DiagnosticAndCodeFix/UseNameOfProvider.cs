using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

#pragma warning disable "CS1998"

namespace DiagnosticAnalyzerAndCodeFix
{
    [ExportCodeFixProvider(UseNameOfAnalyzer.DiagnosticId, LanguageNames.CSharp)]
    public class UseNameOfProvider : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
            => new[] { UseNameOfAnalyzer.DiagnosticId };

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
            => new[] { CodeAction.Create("Use nameof()", c => UseNameOfAsync(document, span, diagnostics.First(), c)) };

        private async Task<Document> UseNameOfAsync(Document document, TextSpan span, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var literal = root.FindToken(diagnosticSpan.Start).Parent.FirstAncestorOrSelf<LiteralExpressionSyntax>();
            var name = literal.Token.ValueText;

            var newNode = SyntaxFactory.NameOfExpression("nameof", SyntaxFactory.ParseTypeName(name));
            var newRoot = root.ReplaceNode(literal, (SyntaxNode)newNode);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
