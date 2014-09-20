using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CompilationDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = @"
                public class Person(string lastName, string firstName)
                {
                    public string LastName { get; } = lastName;
                    public string FirstName { get; } = firstName;
                }";

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code, options: new CSharpParseOptions(languageVersion: LanguageVersion.Experimental));
            var compilation = CSharpCompilation.Create(
                "Person.dll",
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                references: new[] { new MetadataFileReference(typeof(object).Assembly.Location) },
                syntaxTrees: new[] { syntaxTree });

            var diagnostics = compilation.GetDiagnostics();
            foreach (var diagnostic in diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error))
            {
                Console.WriteLine("Error: {0}", diagnostic.GetMessage());
            }

            Assembly assembly;
            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);
                assembly = Assembly.Load(stream.ToArray());
            }

            var testClass = assembly.GetType("Person");
            var testInstance = Activator.CreateInstance(testClass, "Johns", "Martin");

            var lastNameProperty = testClass.GetProperty("LastName");
            var firstNameProperty = testClass.GetProperty("FirstName");

            var lastName = lastNameProperty.GetValue(testInstance);
            var firstName = firstNameProperty.GetValue(testInstance);

            Console.WriteLine("Hello {0} {1}!", firstName, lastName);
            Console.ReadLine();
        }
    }
}
