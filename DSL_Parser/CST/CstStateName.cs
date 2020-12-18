using System;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstStateName : CstCoreNode 
    {
        public string Name { get; }

        public CstStateName(string name)
        {
            Name = name;
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}