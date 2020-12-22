using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstInt : CstConstantValue
    {
        public int Data { get; }

        public CstInt(int data)
        {
            Data = data;
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}