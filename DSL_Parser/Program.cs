using System;
using System.IO;

namespace DSL_Parser
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DSL Parser Lib\n");
            var dslParser = DSLGrammar.GetParser();

            using (var dfaExplanation = File.OpenText("../../../Examples/example1.dfa"))
            {
                var dfaText = dfaExplanation.ReadToEnd().Replace(Environment.NewLine, " ");
                var ast = dslParser.Goal.Parse(dfaText);
                
                Console.WriteLine(ast?.Dot() ?? "[NULL PARSE RESULT]");
            }
        }
    }
}