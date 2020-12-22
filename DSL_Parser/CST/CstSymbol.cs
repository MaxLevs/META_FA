using System;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstSymbol : CstConstantValue
    {
        public string Name { get; }
        
        public CstSymbol(string name)
        {
            Name = name;
        }
        
        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }

        protected bool Equals(CstSymbol other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CstSymbol) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}