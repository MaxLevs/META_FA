using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstCodeArea : CstCoreNode
    {
        public ReadOnlyCollection<CstCodeEntity> Child { get; }

        public CstCodeArea(IList<CstCodeEntity> child) {
            Child = new ReadOnlyCollection<CstCodeEntity>(child);
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}