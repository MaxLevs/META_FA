using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using META_FA.Visitors.CST;

namespace META_FA.CST
{
    public class RegexVariant : RegexCST
    {
        public override Guid Id { get; }
        public ReadOnlyCollection<RegexCST> Children { get; }

        public RegexVariant(IList<RegexCST> children)
        {
            Id = Guid.NewGuid();
            Children = new ReadOnlyCollection<RegexCST>(children);
        }

        public override void Visit(CSTVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}