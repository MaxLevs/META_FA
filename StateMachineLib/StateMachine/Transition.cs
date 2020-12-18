using System;

namespace StateMachineLib.StateMachine
{
    public class Transition
    {
        public Guid Id { get; }
        public string Token { get; }
        public State StartState { get; }
        public State EndState { get; }
        
        public bool IsEpsilon { get; }

        public Transition(State startState, string token, State endState)
        {
            Id = Guid.NewGuid();
            Token = token;
            StartState = startState;
            EndState = endState;
            IsEpsilon = false;
        }
        
        public Transition(State startState, State endState)
        {
            Id = Guid.NewGuid();
            Token = null;
            StartState = startState;
            EndState = endState;
            IsEpsilon = true;
        }

        public Transition ChangeStartState(State newState)
        {
            return IsEpsilon ? new Transition(newState, EndState) : new Transition(newState, Token, EndState);
        }

        public Transition ChangeEndState(State newState)
        {
            return IsEpsilon ? new Transition(StartState, newState) : new Transition(StartState, Token, newState);
        }

        public Transition ChangeToken(string newToken)
        {
            return IsEpsilon ? this : new Transition(StartState, newToken, EndState);
        }

        public override string ToString()
        {
            return $"{(string.IsNullOrEmpty(Token) ? "ε: " : "[" + Token + "]")}{StartState} => {EndState}";
        }
    }
}