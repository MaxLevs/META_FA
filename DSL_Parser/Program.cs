using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            
            var dslParser = DSLGrammar.Build();
            
            Console.WriteLine(dslParser.PrintGrammar() + "\n\n");
            // return;

            using var dfaExplanation = File.OpenText("../../../Examples/future_example.fa");
            var dfaText = dfaExplanation.ReadToEnd();
            // var ast = dslParser.Parse(dfaText);
            var ast = dslParser.Parse("Determine(Nfa1, Dfa1) Minimize(Dfa1, MinDfa1) Function SomeFunc2 (env, int a, bool flag) SomeFunc(Dfa1, 4, 3.14, a, Dfa2) End Function ", DSLGrammar.CodeArea);
                
            Console.WriteLine("Save astOutput.dot into ../../../Examples/astOutput.dot");
            File.Delete("../../../Examples/astOutput.dot");
            using var astFileOutput = File.OpenWrite("../../../Examples/astOutput.dot");
            var astOutputBytes = Encoding.UTF8.GetBytes(ast?.Dot(false) ?? "[NULL PARSE RESULT]");
            astFileOutput.Write(astOutputBytes);
            return;
            
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