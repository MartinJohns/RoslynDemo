using Microsoft.CodeAnalysis.CSharp;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;

namespace FormattingDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = @"
public class Person(string lastName, string firstName) {
    public void Fubar   () {
        // Demo
        int i = 0;
    }
}";

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code, options: new CSharpParseOptions(languageVersion: LanguageVersion.Experimental));
            var codeBeautifier = new CodeBeautifier();
            var beautifiedSyntaxTree = codeBeautifier.Visit(syntaxTree.GetRoot()).NormalizeWhitespace().SyntaxTree;

            Console.WriteLine(beautifiedSyntaxTree.ToString());
            Console.ReadLine();
        }
    }

    public class CodeBeautifier : CSharpSyntaxRewriter
    {
        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            switch (token.CSharpKind())
            {
                case SyntaxKind.OpenBraceToken:
                    if (token.GetPreviousToken().CSharpKind() == SyntaxKind.CloseParenToken)
                    {
                        return token.WithLeadingTrivia(SyntaxFactory.ElasticLineFeed);
                    }
                    break;
            }

            return token;
        }
    }
}
