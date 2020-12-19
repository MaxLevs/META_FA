using System;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstTransition : CstCoreNode
    {
        public CstStateName StartStateName { get; }
        public CstSymbol Token { get; }
        public CstStateName EndStateName { get; }
        public bool IsEpsilon { get; }

        public CstTransition(CstStateName startStateName, CstSymbol token, CstStateName endStateName)
        {
            StartStateName = startStateName;
            Token = token;
            EndStateName = endStateName;
            IsEpsilon = false;
        }

        public CstTransition(CstStateName startStateName, CstStateName endStateName)
        {
            StartStateName = startStateName;
            EndStateName = endStateName;
            IsEpsilon = true;
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }

        protected bool Equals(CstTransition other)
        {
            return Equals(StartStateName, other.StartStateName)
                && Equals(Token, other.Token) 
                && Equals(EndStateName, other.EndStateName) 
                && IsEpsilon == other.IsEpsilon;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CstTransition) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartStateName, Token, EndStateName, IsEpsilon);
        }
    }
}