using System;
using System.Collections.Generic;
using System.Linq;
using META_FA.StateMachine.Exceptions;

namespace META_FA.StateMachine
{
    public class MachineDetermined : Machine
    {
        public override MachineType Type => MachineType.Determined;
        
        protected override void PreAddTransitionCheck(Transition newTransition)
        {
            var foundTransition = _transitions.Find(transition
                => Equals(transition.StartState, newTransition.StartState)
                && transition.Token == newTransition.Token);
            if (foundTransition != null)
            {
                throw new DuplicateTransitionException(newTransition, this);
            }
        }

        protected override bool DoStep(string text, State currentState)
        {
            if (currentState.IsFinal && text == "")
                return true;
            
            var token = text[0].ToString();
            var way = _transitions.Find(transition => Equals(currentState, transition.StartState) && token == transition.Token);

            return way != null && DoStep(text.Substring(1), way.EndState);
        }

        public override Machine Minimize()
        {
            var states_paris_are_not_eq = _states
                .SelectMany(stateOne => _states, (stateOne, stateTwo) => new {stateOne, stateTwo})
                .ToDictionary(statePair => statePair, statePair => Equals(statePair.stateOne, statePair.stateTwo));

            bool repeat;
            do
            {
                repeat = false;
                var suspectedPairs = states_paris_are_not_eq.Where(pair => !pair.Value).ToList();
                var flaggedPairs = states_paris_are_not_eq.Where(pair => pair.Value).ToList();
                foreach (var suspectedPair in suspectedPairs)
                {
                    var foundTransitions = _transitions
                        .Where(tr => Equals(tr.StartState, suspectedPair.Key.stateOne))
                        .SelectMany(
                            trOne => _transitions.Where(tr =>
                                Equals(tr.StartState, suspectedPair.Key.stateTwo) && tr.Token == trOne.Token),
                            (trOne, trTwo) => new {trOne, trTwo});
                    
                    foreach (var transition in foundTransitions)
                    {
                        var newOne = transition.trOne.EndState;
                        var newTwo = transition.trTwo.EndState;
                        
                        if (flaggedPairs.Any(pair =>
                            Equals(pair.Key.stateOne, newOne) && Equals(pair.Key.stateTwo, newTwo)))
                        {
                            states_paris_are_not_eq[suspectedPair.Key] = true;
                            repeat = true;
                            break;
                        }
                    }

                    if (repeat) break;
                }
            } while (repeat);

            var some = states_paris_are_not_eq.Where(pair => !pair.Value).ToList();
            var buffer = new List<State>();
            foreach (var pair in some)
            {
                foreach (var state in _states.Where(state => state.Id == pair.Key.stateTwo.Id))
                {
                    if (!_states.Contains(state)) continue;
                    buffer.Add(state);
                    _states.Add(new State(pair.Key.stateOne.Id, state.IsFinal));
                }
            }

            foreach (var state in buffer)
            {
                _states.Remove(state);
            }
            
            throw new System.NotImplementedException();
        }

        public override MachineDetermined Determine()
        {
            return this;
        }
    }
}