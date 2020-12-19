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

        protected bool Equals(CstString other)
        {
            return Data == other.Data;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CstString) obj);
        }

        public override int GetHashCode()
        {
            return (Data != null ? Data.GetHashCode() : 0);
        }
    }
}