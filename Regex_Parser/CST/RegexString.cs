using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Regex_Parser.Visitors.CST;

namespace Regex_Parser.CST
{
    public class RegexString : RegexCST
    {
        public override Guid Id { get; }
        public ReadOnlyCollection<RegexCST> Children { get; }

        public RegexString(IList<RegexCST> children)
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