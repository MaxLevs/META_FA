using DSL_Parser.CST;

namespace DSL_Parser.Visitors.CST
{
    public abstract class CstCoreVisitor
    {
        public abstract object GetResult();

        public abstract void Apply(CstIdentity cstIdentity);
        public abstract void Apply(CstSymbol cstSymbol);
        public abstract void Apply(CstString cstString);
        public abstract void Apply(CstBool cstBool);
        public abstract void Apply(CstStateName cstStateName);
        public abstract void Apply(CstStatesList cstStatesList);
        public abstract void Apply(CstTransition cstTransition);
        public abstract void Apply(CstDeclaration cstDeclaration);
        public abstract void Apply(CstAsset cstAsset);
        public abstract void Apply(CstDsl cstDsl);

        public abstract void Apply(CstCodeArea cstCodeArea);
        public abstract void Apply(CstDouble cstDouble);
        public abstract void Apply(CstFuncArg cstFuncArg);
        public abstract void Apply(CstFuncDefArg cstFuncDefArg);
        public abstract void Apply(CstFunctionCall cstFunctionCall);
        public abstract void Apply(CstFunctionDefinition cstFunctionDefinition);
        public abstract void Apply(CstInt cstInt);
        
    }
}