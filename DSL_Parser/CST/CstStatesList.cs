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
    }
}