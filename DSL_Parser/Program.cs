using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DSL_Parser
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DSL Parser Lib\n");
            // UpdateRuleDotExamples(); return;
            
            var dslParser = DSLGrammar.GetParser();

            using var dfaExplanation = File.OpenText("Examples/example2.dfa");
            var dfaText = PrepareData(dfaExplanation.ReadToEnd());
            var ast = dslParser.Goal.Parse(dfaText);
                
            Console.WriteLine(ast?.Dot() ?? "[NULL PARSE RESULT]");
        }

        static void UpdateRuleDotExamples()
        {
            // RulesDotExamples
            var dslParser = DSLGrammar.GetParser();

            var assets = new Dictionary<string, string>
            {
                {"Dfa1", DSLGrammar.Identity},
                {"\"some string\"", DSLGrammar.Str},
                {"q", DSLGrammar.Symbol},
                {"state14", DSLGrammar.StateName},
                {"<s1 f s2>", DSLGrammar.TableRow},
                {"<s1 s2>", DSLGrammar.TableRow},
                {"States = ( st1, st2, st3, st4 )", DSLGrammar.StatesBlock},
            };

            foreach (var (asset, ruleName) in assets)
            {
                var ast = dslParser[ruleName].Parse(asset);
                using var resFile = File.CreateText(Path.Join("../../../Examples/RulesDotExamples", $"{ruleName}_example.dot"));
                resFile.Write(ast?.Dot() ?? "[NULL]");
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