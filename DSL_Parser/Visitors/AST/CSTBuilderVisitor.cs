using System.Collections.Generic;
using BFParser;
using DSL_Parser.CST;

namespace DSL_Parser.Visitors.AST
{
    public class CSTBuilderVisitor : IAstCoreVisitor<CSTCoreNode>
    {
        private Stack<CSTCoreNode> nodes;

        public CSTBuilderVisitor()
        {
            nodes = new Stack<CSTCoreNode>();
        }

        public void Apply(SyntaxTreeNode syntaxTreeNode)
        {
            switch (syntaxTreeNode.RuleName)
            {
                case DSLGrammar.Identity:
                {
                    break;
                }

                case DSLGrammar.Symbol:
                {
                    break;
                }

                case DSLGrammar.Str:
                {
                    break;
                }

                case DSLGrammar.Dsl:
                {
                    break;
                }

                case DSLGrammar.AssetArgs:
                {
                    break;
                }

                case DSLGrammar.AssetRule:
                {
                    break;
                }

                case DSLGrammar.AssetsArea:
                {
                    break;
                }

                case DSLGrammar.DeclareArea:
                {
                    break;
                }

                case DSLGrammar.DeclareBody:
                {
                    break;
                }
                
                case DSLGrammar.FinalsBlock:
                {
                    break;
                }
                
                case DSLGrammar.InitialBlock:
                {
                    break;
                }
                
                case DSLGrammar.StateName:
                {
                    break;
                }
                
                case DSLGrammar.StatesBlock:
                {
                    break;
                }
                
                case DSLGrammar.StatesList:
                {
                    break;
                }
                
                case DSLGrammar.TableBlock:
                {
                    break;
                }
                
                case DSLGrammar.TableRow:
                {
                    break;
                }
            }
        }

        public CSTCoreNode GetResult()
        {
            throw new System.NotImplementedException();
        }
    }
}