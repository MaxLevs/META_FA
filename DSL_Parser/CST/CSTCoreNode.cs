using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public abstract class CSTCoreNode
    {
        public abstract void Visit(CSTVisitor visitor);
    }
}