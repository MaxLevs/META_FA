using System;

namespace META_FA.StateMachine
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
    }
}