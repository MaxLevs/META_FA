using System;

namespace StateMachineLib.StateMachine
{
    public class State
    {
        public string Id { get; }
        public bool IsFinal { get; }

        public State(string id, bool isFinal)
        {
            Id = id;
            IsFinal = isFinal;
        }

        public State(bool isFinal = false)
        {
            Id = Guid.NewGuid().ToString().Substring(0,7);
            IsFinal = isFinal;
        }
        
        public override bool Equals(object? obj)
        {
            if (!(obj is State))
                return false;
            
            return Id == ((State) obj).Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, IsFinal);
        }

        public override string ToString()
        {
            return Id;
        }
    }
}