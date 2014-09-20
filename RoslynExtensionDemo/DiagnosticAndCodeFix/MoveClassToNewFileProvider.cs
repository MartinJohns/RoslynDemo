using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiagnosticAnalyzerAndCodeFix
{
    [ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp)]
    public class MoveClassToNewFileProvider : ICodeRefactoringProvider
    {
        public const string RefactoringId = "MoveClassToNewFile";

        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var node = root.FindNode(span);
            var classDeclaration = node as ClassDeclarationSyntax;
            if (classDeclaration == null)
                return null;

            return new[] { CodeAction.Create("Move to new file", c => MoveToNewFile(document, classDeclaration, c)) };
        }

        private async Task<Solution> MoveToNewFile(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            var className = classDeclaration.Identifier.Text;

            var syntaxRoot = await document.GetSyntaxRootAsync();
            var newNamespace = classDeclaration
                .FirstAncestorOrSelf<NamespaceDeclarationSyntax>()
                .WithMembers(SyntaxFactory.List(new[] { (MemberDeclarationSyntax)classDeclaration }));
            var compilationUnit = SyntaxFactory.CompilationUnit()
                .WithMembers(SyntaxFactory.List(new[] { (MemberDeclarationSyntax)newNamespace }))
                .WithUsings(SyntaxFactory.List(syntaxRoot.DescendantNodes().OfType<UsingDirectiveSyntax>()))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var newDocument = document.Project.AddDocument(className + ".cs", string.Empty, folders: document.Folders).WithSyntaxRoot(compilationUnit);
            newDocument = newDocument.Project
                .GetDocument(document.Id)
                .WithSyntaxRoot(syntaxRoot.RemoveNode(classDeclaration, SyntaxRemoveOptions.KeepNoTrivia));

            return newDocument.Project.Solution;
        }
    }
}
