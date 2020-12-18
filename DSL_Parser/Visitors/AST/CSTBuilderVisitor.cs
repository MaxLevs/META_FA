using BFParser;
using BFParser.SyntaxTreeNodeVisitors;

namespace DSL_Parser.Visitors.AST
{
    public class CSTBuilderVisitor : CoreSyntaxTreeNodeVisitor
    {
        public override void Visit(SyntaxTreeNode syntaxTreeNode)
        {
            throw new System.NotImplementedException();
        }

        public override object GetResult()
        {
            throw new System.NotImplementedException();
        }
    }
}