using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstFunctionCall : CstCodeEntity
    {
        public CstIdentity FunctionName { get; }
        public ReadOnlyCollection<CstFuncArg> Args { get; }

        public CstFunctionCall(CstIdentity functionName, IList<CstFuncArg> args)
        {
            FunctionName = functionName;
            Args = new ReadOnlyCollection<CstFuncArg>(args);
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}
