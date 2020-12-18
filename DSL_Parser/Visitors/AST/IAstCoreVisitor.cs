using BFParser;

namespace DSL_Parser.Visitors.AST
{
    public interface IAstCoreVisitor<out TResult>
    {
        public void Apply(SyntaxTreeNode syntaxTreeNode);
        public TResult GetResult();
    }
}