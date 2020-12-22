using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstFunctionDefinition : CstCodeEntity
    {
        public CstIdentity Name { get; }
        public ReadOnlyCollection<CstFuncDefArg> Args { get; }
        public ReadOnlyCollection<CstCoreNode> Statements { get; }

        public CstFunctionDefinition(CstIdentity name, IList<CstFuncDefArg> args, IList<CstCoreNode> statements)
        {
            Name = name;
            Args = new ReadOnlyCollection<CstFuncDefArg>(args);
            Statements = new ReadOnlyCollection<CstCoreNode>(statements);
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}