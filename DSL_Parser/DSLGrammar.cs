using System;
using System.Collections.Generic;
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
        public const string Identity = "identity";
        public const string String = "string";
        public const string Bool = "bool";
        public const string TableRow = "table_row";
        public const string Symbol = "symbol";
        
        public const string FunctionCall = "function_call";
        public const string FuncArgsList = "func_args_list";
        public const string FuncArg = "func_arg";
        public const string FunctionDefinition = "function_definition";
        public const string FuncDefArg = "func_def_arg";
        public const string FuncDefArgsList = "func_def_args_list";
        public const string FunctionBody = "function_body";
        public const string VariableName = "variable_name";
        public const string VariableType = "variable_type";
        public const string ConstantValue = "constant_value";
        public const string Int = "int";
        public const string Double = "double";
        public const string Statement = "statement";

        public const string CodeArea = "code_area";

        public static Grammar Build()
        {
            var gram = new Grammar(Dsl)
            {
                {Identity, P.RE(@"(([A-Z][a-z_]+)+|[A-Z])[0-9_]*")}, // M, MinMax, MinMax_release, Some9
                {Symbol, P.RE(@"[a-zA-Z0-9]")},
                {String, P.RE("\\\"[^\"]*\\\"")},
                {Bool, P.T("true") | P.T("false")},
                {Int, P.RE(@"\d+")},
                {Double, P.RE(@"\d+\.\d*")},
                {ConstantValue, P.C(String) | P.C(Bool) | P.C(Double) | P.C(Int) | P.C(Symbol)},
                {VariableType, P.T(String) | P.T(Bool) | P.T(Symbol) | P.T(Double) | P.T(Int)},
                {VariableName, P.RE("[a-z_]?[a-z]+")},
                
                {StateName, P.RE(@"[a-z]+[0-9]*")},
                {StatesList, P.C(StateName) + P.T(",") + P.C(StatesList) | P.C(StateName)},
                {DeclareArea, P.T("Declare") + P.C(Identity) + P.C(DeclareBody) + P.T(new[] {"End", "Declare"})},
                {DeclareBody, P.MB(StatesBlock) + P.C(InitialBlock) + P.C(FinalsBlock) + P.C(TableBlock)},
                {StatesBlock, P.T(new[] {"States", "=", "("}) + P.C(StatesList) + P.T(")")},
                {InitialBlock, P.T(new[] {"Initial", "="}) + P.C(StateName)},
                {FinalsBlock, P.T(new[] {"Finals", "=", "("}) + P.C(StatesList) + P.T(")")},
                {TableBlock, P.T(new[] {"Table", "=", "("}) + P.OI(TableRow) + P.T(")")},
                {TableRow, P.T("<") + ((P.C(StateName) + P.C(Symbol) + P.C(StateName)) | P.C(StateName) + P.C(StateName)) + P.T(">")},
                
                {FunctionCall, P.C(Identity) + P.T("(") + P.C(FuncArgsList) + P.T(")")},
                {FuncArgsList, P.C(FuncArg) + P.T(",") + P.C(FuncArgsList) | P.C(FuncArg)},
                {FuncArg, P.C(Identity) | P.C(ConstantValue)},
                
                {FunctionDefinition, P.T("Function") + P.C(Identity) + P.T("(") + P.MB(FuncDefArgsList) + P.T(")") + P.MB(FunctionBody) + P.T("End") + P.T("Function")},
                {FuncDefArgsList, P.C(FuncDefArg) + P.T(",") + P.C(FuncDefArgsList) | P.C(FuncDefArg)},
                {FuncDefArg, P.MB(VariableType) + P.C(VariableName)},
                {FunctionBody, P.OI(Statement)},
                {Statement, P.C(FunctionCall)},
                
                {CodeArea, P.OI(P.C(FunctionDefinition) | P.C(FunctionCall))},
                {Dsl, P.OI(DeclareArea) + P.MB(CodeArea)},
            };

            gram.CommentDefinition = new List<string> { @"/\*.*\*/", $"//[^\n]*\n" };
            gram.InitGrammar();
        
            return gram;
        }
    }
}