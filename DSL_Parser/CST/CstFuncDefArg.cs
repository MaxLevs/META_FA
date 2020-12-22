using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstFuncDefArg : CstCoreNode
    {
        public string Type { get; }
        public string Name { get; }

        public CstFuncDefArg(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}