using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstFuncArgsList : CstCoreNode
    {
        public ReadOnlyCollection<CstFuncArg> Args { get; }

        public CstFuncArgsList(IList<CstFuncArg> args)
        {
            Args = new ReadOnlyCollection<CstFuncArg>(args);
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            throw new System.NotImplementedException();
        }
    }
}