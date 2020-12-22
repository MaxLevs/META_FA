using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstFuncArg : CstCoreNode
    {
        public string Type => Data switch {
            CstIdentity _ => DSLGrammar.Identity,
            CstBool     _ => DSLGrammar.Bool,
            CstString   _ => DSLGrammar.String,
            CstSymbol   _ => DSLGrammar.Symbol,
            CstDouble   _ => DSLGrammar.Double,
            CstInt      _ => DSLGrammar.Int,
            // todo: add another types
        };
        
        public CstCoreNode Data { get; }

        public CstFuncArg(CstCoreNode data)
        {
            Data = data;
        }

        public override void Visit(CstCoreVisitor visitor)
        { 
            visitor.Apply(this);
        }
    }
}