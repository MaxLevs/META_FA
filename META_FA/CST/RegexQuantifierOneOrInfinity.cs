using System;
using META_FA.Visitors.CST;

namespace META_FA.CST
{
    public class RegexQuantifierOneOrInfinity : RegexCST
    {
        public override Guid Id { get; }
        public RegexCST Children { get; }

        public RegexQuantifierOneOrInfinity(RegexCST children)
        {
            Id = Guid.NewGuid();
            Children = children;
        }

        public override void Visit(CSTVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}