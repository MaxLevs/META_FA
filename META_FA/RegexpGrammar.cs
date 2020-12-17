using System.Collections.Generic;
using BFParser;
using BFParser.Parsers;
using BFParser.Parsers.Combinators;

namespace META_FA
{
    public static class RegexpGrammar
    {
        public static Grammar GetParser()
        {
            var gram = new Grammar("string")
            {
                {"symbol", P.RE(@".")},
                {"quantifier", P.C("element") + P.T("?") |
                               P.C("element") + P.T("*") |
                               P.C("element")},
                {"element", P.T("(") + P.C("variant") + P.T(")")},
                {"variant", P.C("string") + P.T("|") + P.C("variant") | P.C("string")},
                {"string", P.OI(P.C("quantifier"))}
            };
            gram.InitGrammar();
        
            return gram;
        }
    }
}