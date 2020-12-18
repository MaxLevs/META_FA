using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public abstract class CSTCore
    {
        public abstract void Visit(CSTVisitor visitor);
    }
}