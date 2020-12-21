using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DSL_Parser.CST;
using DSL_Parser.Visitors.AST;

namespace DSL_Parser
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DSL Parser Lib\n");
            Console.WriteLine();
            
            // UpdateRuleDotExamples(); return;
            
            var dslParser = DSLGrammar.Build();
            
            Console.WriteLine(dslParser.PrintGrammar() + "\n\n"); return;

            using var dfaExplanation = File.OpenText("Examples/example2.dfa");
            var dfaText = PrepareData(dfaExplanation.ReadToEnd());
            var ast = dslParser.Parse(dfaText);
                
            // Console.WriteLine(ast?.Dot() ?? "[NULL PARSE RESULT]");
            
            var cstBuilder = new CstBuilderVisitor();
            cstBuilder.Apply(ast);
            var cstDsl = (CstDsl) cstBuilder.GetResult();
            
            Console.WriteLine(cstDsl?.Dot() ?? "[NULL]");
        }

        private static string PrepareData(string data)
        {
            var withoutNewLines = data.Replace(Environment.NewLine, " ");
            var withoutMultipleSpaces = new Regex("[ ]{2,}", RegexOptions.None).Replace(withoutNewLines, " ");
            return withoutMultipleSpaces;
        }
    }
}