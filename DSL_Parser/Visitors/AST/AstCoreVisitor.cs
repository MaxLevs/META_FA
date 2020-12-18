using BFParser;

namespace DSL_Parser.Visitors.AST
{
    public abstract class AstCoreVisitor
    {
        public abstract void Apply(SyntaxTreeNode syntaxTreeNode);
        public abstract object GetResult();
    }
}