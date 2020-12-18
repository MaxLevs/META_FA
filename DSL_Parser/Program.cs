using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DSL_Parser
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DSL Parser Lib\n");
            var dslParser = DSLGrammar.GetParser();

            using var dfaExplanation = File.OpenText("../../../Examples/example2.dfa");
            var dfaText = PrepareData(dfaExplanation.ReadToEnd());
            var ast = dslParser.Goal.Parse(dfaText);
                
            Console.WriteLine(ast?.Dot() ?? "[NULL PARSE RESULT]");
        }

        private static string PrepareData(string data)
        {
            var withoutNewLines = data.Replace(Environment.NewLine, " ");
            var withoutMultipleSpaces = new Regex("[ ]{2,}", RegexOptions.None).Replace(withoutNewLines, " ");
            return withoutMultipleSpaces;
        }
    }
}