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

        protected bool Equals(CstDsl other)
        {
            return Equals(Declarations, other.Declarations)
                && Equals(Assets, other.Assets);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CstDsl) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Declarations, Assets);
        }
    }
}