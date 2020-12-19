using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DSL_Parser;
using DSL_Parser.CST;
using DSL_Parser.Visitors.AST;

namespace StateMachineLib.Options
{
    public class Options
    {
        public SMOptions Arch { get; set; }
        public List<Asset> Assets { get; set; }

        public static List<Options> FromFile(string path)
        {
            using var file = File.OpenText(path);
            var text = file.ReadToEnd();
            file.Close();
            
            var ast = DSLGrammar.GetParser().Goal.Parse(text);
            
            if (ast == null) throw new NotImplementedException("Implement custom exception: Config read error");
            
            var cstBuilder = new CstBuilderVisitor();
            cstBuilder.Apply(ast);
            var cst = (CstDsl) cstBuilder.GetResult();
            
            var optionsBuilder = new DslStateMachineOptionsBuilderVisitor();
            cst.Visit(optionsBuilder);
            
            var options = (Dictionary<string, Options>) optionsBuilder.GetResult();

            return options.Values.ToList();
        }

        public string ToText()
        {
            throw new NotImplementedException();
        }

        public void ToFile(string path)
        {
            throw new NotImplementedException();
        }
    }
}