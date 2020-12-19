using System;
using System.Diagnostics.CodeAnalysis;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public abstract class CstCoreNode
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string IdShort => Id.ToString().Substring(0, 7);
        public abstract void Visit(CstCoreVisitor visitor);
    }
}