using System;
using META_FA.Visitors.CST;

namespace META_FA.CST
{
    public class RegexQuantifierOneOrInfinity : RegexCST
    {
        public override Guid Id { get; }
        public RegexCST Child { get; }

        public RegexQuantifierOneOrInfinity(RegexCST child)
        {
            Id = Guid.NewGuid();
            Child = child;
        }

        public override void Visit(CSTVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}