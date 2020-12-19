using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstStatesList : CstCoreNode
    {
        public ReadOnlyCollection<CstStateName> StateNames { get; }

        public CstStatesList(IList<CstStateName> stateNames)
        {
            StateNames = new ReadOnlyCollection<CstStateName>(stateNames);
        }
        
        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }

        protected bool Equals(CstStatesList other)
        {
            return Equals(StateNames, other.StateNames);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CstStatesList) obj);
        }

        public override int GetHashCode()
        {
            return (StateNames != null ? StateNames.GetHashCode() : 0);
        }
    }
}