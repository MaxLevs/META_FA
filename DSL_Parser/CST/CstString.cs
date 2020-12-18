using System;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstString : CstCoreNode
    {
        public string Data { get; }
        
        public CstString(string data)
        {
            Data = data;
        }
        
        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}