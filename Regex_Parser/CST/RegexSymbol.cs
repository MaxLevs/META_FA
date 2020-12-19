using System;
using Regex_Parser.Visitors.CST;

namespace Regex_Parser.CST
{
    public class RegexSymbol : RegexCST
    {
        public override Guid Id { get; }
        public string Token { get; }

        public RegexSymbol(string token)
        {
            Id = Guid.NewGuid();
            Token = token;
        }

        public override void Visit(CSTVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}