using DiagnosticAnalyzerAndCodeFix;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RoslynExtensionDemo.Test
{

    [TestClass]
    public class UnitTests : CodeFixVerifier
    {
        [TestMethod]
        public void NoDiagnosticShowingUp()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void UseNameOfAnalyzerAndCodeFix()
        {
            var code = @"
                namespace ConsoleApplication1
                {
                    public class Program
                    {
                        public static void Main(string[] args)
                        {
                            var str = ""Program"";
                        }
                    }
                }";

            var expected = new DiagnosticResult
            {
                Id = UseNameOfAnalyzer.DiagnosticId,
                Message = "String literals are easy to get wrong. Consider using the nameof() operator instead.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 8, 39)
                }
            };

            VerifyCSharpDiagnostic(code, expected);

            var fixedCode = @"
                namespace ConsoleApplication1
                {
                    public class Program
                    {
                        public static void Main(string[] args)
                        {
                            var str = nameof(Program);
                        }
                    }
                }";

            VerifyCSharpFix(code, fixedCode);
        }

        protected override IDiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseNameOfAnalyzer();
        }

        protected override ICodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UseNameOfProvider();
        }
    }
}