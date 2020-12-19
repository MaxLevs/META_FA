using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSL_Parser.Visitors.CST;

namespace DSL_Parser.CST
{
    public class CstDeclaration : CstCoreNode
    {
        public CstIdentity Identity { get; }
        public CstStateName InitialStateName { get; }
        public ReadOnlyCollection<CstStateName> States { get; }
        public ReadOnlyCollection<CstStateName> Finals { get; }
        public ReadOnlyCollection<CstTransition> Trancitions { get; }

        public CstDeclaration(CstIdentity identity, IList<CstStateName> states, CstStateName initialStateName, IList<CstStateName> finals, IList<CstTransition> trancitions)
        {
            Identity = identity;
            States = states == null ? null : new ReadOnlyCollection<CstStateName>(states);
            InitialStateName = initialStateName;
            Finals = new ReadOnlyCollection<CstStateName>(finals);
            Trancitions = new ReadOnlyCollection<CstTransition>(trancitions);
        }

        public override void Visit(CstCoreVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}