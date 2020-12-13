using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using META_FA.StateMachine.Exceptions;

namespace META_FA.StateMachine
{
    public class StateMachine
    {
        public Guid Id { get; } = Guid.NewGuid();
        private List<State> _states = new List<State>();
        private List<Transition> _transitions = new List<Transition>();
        public State CurrentState = null;

        public void AddState(State newState)
        {
            var foundState = _states.Find(state => state.Id == newState.Id);
            if (foundState != null)
            {
                throw new DuplicateStateException(newState, this);
            }
            _states.Add(newState);
        }

        public void AddTransition(Transition newTransition)
        {
            var foundTransition = _transitions.Find(transition
                => transition.StartState.Id == newTransition.StartState.Id
                && transition.EndState.Id == newTransition.EndState.Id
                && transition.Token == newTransition.Token);
            if (foundTransition != null)
            {
                throw new DuplicateTransitionException(newTransition, this);
            }
            
            _transitions.Add(newTransition);
        }

        public void Init(string initialStateId)
        {
            var initialState = _states.Find(state => state.Id == initialStateId);
            CurrentState = initialState ?? throw new InitialStateIsNullException(initialStateId, this);

            var unreachableStates = _states
                .Select(state => _transitions.Find(transition => transition.EndState.Id == state.Id))
                .Any(foundTransition => foundTransition == null);
            var oblivionWayTransitions = _transitions
                .Select(transition => _states.Find(state => state.Id == transition.EndState.Id))
                .Any(foundState => foundState == null);
            if (oblivionWayTransitions)
            {
                throw new OblivionWayTransitionsException(this);
            }

            if (unreachableStates)
            {
                Console.WriteLine($"[Warning] There is some unreachable states into machine: {Id}");
                var noAnyReachableFinalState = _states
                    .Where(state => state.IsFinal)
                    .Select(state => _transitions.Find(transition => transition.EndState.Id == state.Id))
                    .Any(foundTransition => foundTransition == null);
                if (noAnyReachableFinalState)
                {
                    throw new NoAnyReachableFinalStateException(this);
                }
            }
        }
    }
}