using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using META_FA.Visitors.CST;

namespace META_FA.CST
{
    public class RegexString : RegexCST
    {
        public override Guid Id { get; }
        public ReadOnlyCollection<RegexCST> Child { get; }

        public RegexString(IList<RegexCST> child)
        {
            Id = Guid.NewGuid();
            Child = new ReadOnlyCollection<RegexCST>(child);
        }

        public override void Visit(CSTVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}