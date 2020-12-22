using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstDouble : CstConstantValue
    {
        public double Data { get; }
        
        public CstDouble(double data)
        {
            Data = data;
        }
        
        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}