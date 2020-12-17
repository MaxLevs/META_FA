#nullable enable
using System;
using META_FA.Visitors.CST;

namespace META_FA.CST
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
    }
}