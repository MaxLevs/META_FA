using BFParser;
using BFParser.SyntaxTreeNodeVisitors;
using META_FA.CST;

namespace META_FA.Visitors.AST
{
    public class CSTBuilderVisitor : CoreSyntaxTreeNodeVisitor
    {
        
        public override void Visit(SyntaxTreeNode syntaxTreeNode)
        {
            switch (syntaxTreeNode.RuleName)
            {
                case RegexpGrammar.Symbol:
                    break;
                
                case RegexpGrammar.Element:
                    break;
                
                case RegexpGrammar.Quantifier:
                    break;
                
                case RegexpGrammar.Variant:
                    break;
                
                case RegexpGrammar.Str:
                    break;
            }
        }

        public override object GetResult()
        {
            throw new System.NotImplementedException();
        }
    }
}