using System;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstAsset : CstCoreNode
    {
        public CstIdentity Identity { get; }
        public string @String { get; }

        public CstAsset(CstIdentity identity, string @string)
        {
            Identity = identity;
            String = @string;
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}