using System;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstAsset : CstCoreNode
    {
        public CstIdentity Identity { get; }
        public string @String { get; }
        public bool ExpectedResult { get; }

        public CstAsset(CstIdentity identity, string @string, bool expectedResult)
        {
            Identity = identity;
            String = @string;
            ExpectedResult = expectedResult;
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }

        protected bool Equals(CstAsset other)
        {
            return Equals(Identity, other.Identity)
                && String == other.String;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CstAsset) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identity, String);
        }
    }
}