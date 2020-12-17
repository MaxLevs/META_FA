using System;
using META_FA.Visitors.CST;

namespace META_FA.CST
{
    public abstract class RegexCST
    {
        public abstract Guid Id { get; }
        public abstract void Visit(CSTVisitor visitor);
    }
}