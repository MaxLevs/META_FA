using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstDsl : CstCoreNode
    {
        public ReadOnlyCollection<CstDeclaration> Declarations { get; }
        public ReadOnlyCollection<CstAsset> Assets { get; }

        public CstDsl(IList<CstDeclaration> declarations, IList<CstAsset> assets)
        {
            Declarations = new ReadOnlyCollection<CstDeclaration>(declarations);
            Assets = new ReadOnlyCollection<CstAsset>(assets);
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}