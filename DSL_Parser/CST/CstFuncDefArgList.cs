using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstFuncDefArgList : CstCoreNode
    {
        public ReadOnlyCollection<CstFuncDefArg> Args { get; }

        public CstFuncDefArgList(IList<CstFuncDefArg> args)
        {
            Args = new ReadOnlyCollection<CstFuncDefArg>(args);
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            throw new System.NotImplementedException();
        }
    }
}