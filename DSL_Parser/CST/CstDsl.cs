using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstDsl : CstCoreNode
    {
        public ReadOnlyCollection<CstDeclaration> Declarations { get; }
        public ReadOnlyCollection<CstCodeEntity> CodeEntities { get; }

        public CstDsl(IList<CstDeclaration> declarations, IList<CstCodeEntity> codeEntities)
        {
            Declarations = new ReadOnlyCollection<CstDeclaration>(declarations);
            CodeEntities = new ReadOnlyCollection<CstCodeEntity>(codeEntities);
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }

        protected bool Equals(CstDsl other)
        {
            return Equals(Declarations, other.Declarations)
                && Equals(CodeEntities, other.CodeEntities);
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
            return HashCode.Combine(Declarations, CodeEntities);
        }
    }
}