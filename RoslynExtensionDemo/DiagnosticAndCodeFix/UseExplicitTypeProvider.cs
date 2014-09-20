using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;

namespace DiagnosticAnalyzerAndCodeFix
{
    [ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp)]
    public class UseExplicitTypeProvider : ICodeRefactoringProvider
    {
        public const string RefactoringId = "UseExplicitType";

        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var node = root.FindNode(span);
            var identifierName = node as IdentifierNameSyntax;
            if (identifierName == null || !identifierName.IsVar)
                return null;

            var semanticModel = await document.GetSemanticModelAsync();
            var typeInfo = semanticModel.GetTypeInfo(identifierName);
            if (!typeInfo.ConvertedType.CanBeReferencedByName)
                return null;

            return new[] { CodeAction.Create("Use explicit type", c => UseTypeExplicit(document, identifierName, typeInfo, c)) };
        }

        private async Task<Solution> UseTypeExplicit(Document document, IdentifierNameSyntax identifierName, TypeInfo typeInfo, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync();
            var newIdentifierName =
                SyntaxFactory.ParseTypeName(typeInfo.ConvertedType.ToString())
                    .WithAdditionalAnnotations(Simplifier.Annotation)
                    .WithLeadingTrivia(identifierName.GetLeadingTrivia())
                    .WithTrailingTrivia(identifierName.GetTrailingTrivia());

            var newDocument = document.WithSyntaxRoot(root.ReplaceNode(identifierName, newIdentifierName));
            return newDocument.Project.Solution;
        }
    }
}
