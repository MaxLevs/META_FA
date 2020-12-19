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

        protected bool Equals(CstDeclaration other)
        {
            return Equals(Identity, other.Identity)
                && Equals(InitialStateName, other.InitialStateName) 
                && Equals(States, other.States) 
                && Equals(Finals, other.Finals) 
                && Equals(Trancitions, other.Trancitions);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CstDeclaration) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identity, InitialStateName, States, Finals, Trancitions);
        }
    }
}