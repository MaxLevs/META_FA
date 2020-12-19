using System.Collections.Generic;
using DSL_Parser.CST;
using DSL_Parser.Visitors.CST;
using StateMachineLib.Options;

namespace META_FA
{
    public class DslStateMachineOptionsBuilder : CstCoreVisitor
    {
        private readonly List<Options> _options;

        public DslStateMachineOptionsBuilder()
        {
            _options = new List<Options>();
        }

        public override object GetResult()
        {
            return _options;
        }

        public override void Apply(CstIdentity cstIdentity)
        {
            throw new System.NotImplementedException();
        }

        public override void Apply(CstSymbol cstSymbol)
        {
            throw new System.NotImplementedException();
        }

        public override void Apply(CstString cstString)
        {
            throw new System.NotImplementedException();
        }

        public override void Apply(CstStateName cstStateName)
        {
            throw new System.NotImplementedException();
        }

        public override void Apply(CstStatesList cstStatesList)
        {
            throw new System.NotImplementedException();
        }

        public override void Apply(CstTransition cstTransition)
        {
            throw new System.NotImplementedException();
        }

        public override void Apply(CstDeclaration cstDeclaration)
        {
            throw new System.NotImplementedException();
        }

        public override void Apply(CstAsset cstAsset)
        {
            throw new System.NotImplementedException();
        }

        public override void Apply(CstDsl cstDsl)
        {
            throw new System.NotImplementedException();
        }
    }
}