using META_FA.CST;

namespace META_FA.Visitors.CST
{
    public abstract class CSTVisitor
    {
        public abstract void Apply(RegexCST node);
    }
}