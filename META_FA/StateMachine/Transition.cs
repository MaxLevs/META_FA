using System;

namespace META_FA.StateMachine
{
    public class Transition
    {
        public Guid Id { get; }
        public string Token { get; }
        public State StartState { get; }
        public State EndState { get; }

        public Transition(State startState, string token, State endState)
        {
            Id = Guid.NewGuid();
            Token = token;
            StartState = startState;
            EndState = endState;
        }
    }
}