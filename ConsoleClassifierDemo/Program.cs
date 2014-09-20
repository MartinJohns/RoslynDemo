using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleClassifierDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            var workspace = new CustomWorkspace();
            var solution = workspace.CurrentSolution;
            var project = solution.AddProject("projectName", "assemblyName", LanguageNames.CSharp);
            var document = project.AddDocument("name.cs",
                @"using System;
public namespace Demo{public static class Program
    {public static void Main()
        {Console.WriteLine(""Hello Roslyn!"");
}}}");

            document = await Formatter.FormatAsync(document);
            var source = await document.GetTextAsync();

            var classifiedSpans = await Classifier.GetClassifiedSpansAsync(document, TextSpan.FromBounds(0, source.Length));
            Console.BackgroundColor = ConsoleColor.Black;

            var ranges = classifiedSpans.Select(x => new Range(x, source.GetSubText(x.TextSpan).ToString()));

            ranges = FillGaps(source, ranges);

            foreach (var range in ranges)
            {
                switch (range.ClassificationType)
                {
                    case "keyword":
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;
                    case "class name":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case "string":
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    case "punctuation":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case "operator":
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                    case "identifier":
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        break;
                }

                Console.Write(range.Text);
            }

            Console.ResetColor();
            Console.ReadLine();
        }

        private static IEnumerable<Range> FillGaps(SourceText text, IEnumerable<Range> ranges)
        {
            const string WhitespaceClassification = null;
            int current = 0;
            Range previous = null;

            foreach (Range range in ranges)
            {
                int start = range.TextSpan.Start;
                if (start > current)
                {
                    yield return new Range(WhitespaceClassification, TextSpan.FromBounds(current, start), text);
                }

                if (previous == null || range.TextSpan != previous.TextSpan)
                {
                    yield return range;
                }

                previous = range;
                current = range.TextSpan.End;
            }

            if (current < text.Length)
            {
                yield return new Range(WhitespaceClassification, TextSpan.FromBounds(current, text.Length), text);
            }
        }

        public class Range(ClassifiedSpan classifiedSpan, string text)
        {
            public Range(string classification, TextSpan span, SourceText text) :
                this(classification, span, text.GetSubText(span).ToString())
            { }

            public Range(string classification, TextSpan span, string text) :
                this(new ClassifiedSpan(classification, span), text)
            { }

            public string ClassificationType { get; } = classifiedSpan.ClassificationType;
            public TextSpan TextSpan { get; } = classifiedSpan.TextSpan;
            public string Text { get; } = text;
        }
    }
}
