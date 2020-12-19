using System;
using Regex_Parser.Visitors.CST;

namespace Regex_Parser.CST
{
    public class RegexQuantifierMaybe : RegexCST
    {
        public override Guid Id { get; }
        public RegexCST Child { get; }

        public RegexQuantifierMaybe(RegexCST child)
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