using System.Collections.Generic;
using BFParser;
using BFParser.Parsers;
using BFParser.Parsers.Combinators;

namespace META_FA
{
    public static class RegexpGrammar
    {
        // Rule names
        public const string Symbol = "symbol";
        public const string Quantifier = "quantifier";
        public const string Element = "element";
        public const string Variant = "variant";
        public const string Str = "str";

        public static Grammar GetParser()
        {
            var gram = new Grammar(Str)
            {
                {Symbol, P.RE(@"[^*?()+|]")},
                {Quantifier, P.C(Element) + P.T("?") |
                             P.C(Element) + P.T("*") |
                             P.C(Element) + P.T("+") |
                             P.C(Element)},
                {Element, P.T("(") + P.C(Variant) + P.T(")") | P.C(Symbol)},
                {Variant, P.C(Str) + P.T("|") + P.C(Variant) | P.C(Str)},
                {Str, P.OI(P.C(Quantifier))}
            };
            gram.InitGrammar();
        
            return gram;
        }
    }
}