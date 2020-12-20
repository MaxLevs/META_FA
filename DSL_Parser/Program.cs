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
            var ast = dslParser.Goal.Parse(dfaText);
                
            // Console.WriteLine(ast?.Dot() ?? "[NULL PARSE RESULT]");
            
            var cstBuilder = new CstBuilderVisitor();
            cstBuilder.Apply(ast);
            var cstDsl = (CstDsl) cstBuilder.GetResult();
            
            Console.WriteLine(cstDsl?.Dot() ?? "[NULL]");
        }

        static void UpdateRuleDotExamples()
        {
            // RulesDotExamples
            var dslParser = DSLGrammar.Build();

            var assets = new Dictionary<string, string>
            {
                {"Dfa1", DSLGrammar.Identity},
                {"\"some string\"", DSLGrammar.Str},
                {"q", DSLGrammar.Symbol},
                {"state14", DSLGrammar.StateName},
                {"<s1 f s2>", DSLGrammar.TableRow},
                {"States = ( st1, st2, st3, st4 )", DSLGrammar.StatesBlock},
                {"Initial = m1", DSLGrammar.InitialBlock},
                {"Finals = (m5)", DSLGrammar.FinalsBlock},
                {"Table = ( <m1 a m2> <m1 b m3> <m1 c m4> <m2 b m1> <m3 c m1> <m4 b m5> )", DSLGrammar.TableBlock},
                {"Declare Dfa1 Initial = m1 Finals = (m5) Table = ( <m1 a m2> <m1 b m3> <m1 c m4> <m2 b m1> <m3 c m1> <m4 b m5> ) End Declare", DSLGrammar.InitialBlock},
                {"Run(Dfa1, \"cb\")", DSLGrammar.AssetRule},
                {"Run(Dfa1, \"cb\") Run(Dfa1, \"acb\") Run(Dfa1, \"abcb\")", DSLGrammar.AssetsArea},
                {"Declare Dfa1 Initial = m1 Finals = (m5) Table = ( <m1 a m2> <m1 b m3> <m1 c m4> <m2 b m1> <m3 c m1> <m4 b m5> ) End Declare Run(Dfa1, \"cb\") Run(Dfa1, \"acb\") Run(Dfa1, \"abcb\")", DSLGrammar.Dsl}
            };

            foreach (var (asset, ruleName) in assets)
            {
                var ast = dslParser[ruleName].Parse(asset);
                using var resFile = File.CreateText(Path.Join("../../../Examples/RulesDotExamples", $"{ruleName}_example.dot"));
                var astDot = ast?.Dot();
                resFile.Write(astDot ?? "[NULL]");
            }
        }

        private static string PrepareData(string data)
        {
            var withoutNewLines = data.Replace(Environment.NewLine, " ");
            var withoutMultipleSpaces = new Regex("[ ]{2,}", RegexOptions.None).Replace(withoutNewLines, " ");
            return withoutMultipleSpaces;
        }
    }
}