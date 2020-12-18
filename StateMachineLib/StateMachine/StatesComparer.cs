using System.Collections.Generic;

namespace StateMachineLib.StateMachine
{
    class StatesComparer : IEqualityComparer<State>{
        public bool Equals(State x, State y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(State obj)
        {
            return (obj.Id != null ? obj.Id.GetHashCode() : 0);
        }
    }
}