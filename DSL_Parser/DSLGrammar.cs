using BFParser;
using BFParser.Parsers;

namespace DSL_Parser
{
    public static class DSLGrammar
    {
        // Rule names
        public const string Dsl = "dsl";
        public const string DeclareArea = "declare_area";
        public const string DeclareBody = "declare_body";
        public const string StatesBlock = "states_block";
        public const string FinalsBlock = "finals_block";
        public const string TableBlock = "table_block";
        public const string AssetsArea = "assets_area";
        public const string AssetRule = "asset_rule";
        public const string AssetArgs = "asset_args";
        public const string Identity = "identity";
        public const string Str = "str";

        public static Grammar GetParser()
        {
            var gram = new Grammar(Dsl)
            {
                {AssetRule, P.T("Run") + P.T("(") + P.C(AssetArgs) + P.T(")")},
                {AssetArgs, P.C(Identity) + P.T(",") + P.C(Str)},
                {Identity, P.RE(@"[A-Z][a-z]*[0-9]*")},
                {Str, P.RE("\\\"[^\"]*\\\"")},
                {AssetsArea, P.OI(AssetRule)},
            };
            gram.InitGrammar();
        
            return gram;
        }
    }
}