using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstBool : CstConstantValue
    {
        public bool Data { get; }
        
        public CstBool(bool data)
        {
            Data = data;
        }
        
        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }

        protected bool Equals(CstBool other)
        {
            return Data == other.Data;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CstBool) obj);
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }
    }
}