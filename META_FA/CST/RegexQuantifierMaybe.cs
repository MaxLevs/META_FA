using System;
using META_FA.Visitors.CST;

namespace META_FA.CST
{
    public class RegexQuantifierMaybe : RegexCST
    {
        public override Guid Id { get; }
        public RegexCST Children { get; }

        public RegexQuantifierMaybe(RegexCST children)
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