#nullable enable
using System;
using Regex_Parser.Visitors.CST;

namespace Regex_Parser.CST
{
    public abstract class RegexCST
    {
        public abstract Guid Id { get; }
        public abstract void Visit(CSTVisitor visitor);

        public override bool Equals(object? obj)
        {
            return obj is RegexCST regexCst && Equals(Id, regexCst.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public string Dot()
        {
            var dotVisitor = new CSTDotVisitor();
            Visit(dotVisitor);
            
            return dotVisitor.Result;
        }
    }
}