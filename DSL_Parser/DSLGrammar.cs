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
        public const string StatesList = "states_list";
        public const string StateName = "state_name";
        public const string InitialBlock = "initial_block";
        public const string FinalsBlock = "finals_block";
        public const string TableBlock = "table_block";
        public const string AssetsArea = "assets_area";
        public const string AssetRule = "asset_rule";
        public const string AssetArgs = "asset_args";
        public const string Identity = "identity";
        public const string Str = "str";
        public const string Bool = "bool";
        public const string TableRow = "table_row";
        public const string Symbol = "symbol";

        public static Grammar Build()
        {
            var gram = new Grammar(Dsl)
            {
                {Identity, P.RE(@"(([A-Z][a-z_]+)+|[A-Z])[0-9_]*")}, // M, MinMax, MinMax_release, Some9
                {Symbol, P.RE(@"[a-zA-Z0-9]")},
                {Str, P.RE("\\\"[^\"]*\\\"")},
                {Bool, P.T("true") | P.T("false")},
                {StateName, P.RE(@"[a-z]+[0-9]*")},
                {StatesList, P.C(StateName) + P.T(",") + P.C(StatesList) | P.C(StateName)},
                {DeclareArea, P.T("Declare") + P.C(Identity) + P.C(DeclareBody) + P.T(new[] {"End", "Declare"})},
                {DeclareBody, P.MB(StatesBlock) + P.C(InitialBlock) + P.C(FinalsBlock) + P.C(TableBlock)},
                {StatesBlock, P.T(new[] {"States", "=", "("}) + P.C(StatesList) + P.T(")")},
                {InitialBlock, P.T(new[] {"Initial", "="}) + P.C(StateName)},
                {FinalsBlock, P.T(new[] {"Finals", "=", "("}) + P.C(StatesList) + P.T(")")},
                {TableBlock, P.T(new[] {"Table", "=", "("}) + P.OI(TableRow) + P.T(")")},
                {TableRow, P.T("<") + ((P.C(StateName) + P.C(Symbol) + P.C(StateName)) | P.C(StateName) + P.C(StateName)) + P.T(">")},
                {AssetsArea, P.OI(AssetRule)},
                {AssetRule, P.T(new[] {"Asset", "("}) + P.C(AssetArgs) + P.T(")")},
                {AssetArgs, P.C(Identity) + P.T(",") + P.C(Str) + P.T(",") + P.C(Bool)},
                {Dsl, P.OI(DeclareArea) + P.MB(AssetsArea)},
            };
            gram.InitGrammar();
        
            return gram;
        }
    }
}