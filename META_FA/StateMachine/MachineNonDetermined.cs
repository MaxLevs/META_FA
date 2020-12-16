using System.Collections.Generic;
using System.Linq;
using META_FA.StateMachine.Exceptions;

namespace META_FA.StateMachine
{
    public class MachineNonDetermined : Machine
    {
        public override MachineType Type => MachineType.NonDetermined;
        protected override void PreAddTransitionCheck(Transition newTransition)
        {
            var foundTransition = _transitions.Find(transition
                => Equals(transition.StartState, newTransition.StartState)
                && Equals(transition.EndState, newTransition.EndState)
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
            var ways = _transitions.Where(transition => Equals(transition.StartState, currentState) && (transition.IsEpsilon || transition.Token == token));

            return ways.Any(variant => DoStep(text.Substring(1), variant.EndState));
        }
        
        public override Machine Minimize()
        {
            throw new System.NotImplementedException();
        }

        public override MachineDetermined Determine()
        {
            var determined = new MachineDetermined();
            try
            {
                foreach (var state in _states)
                {
                    determined.AddState(state);
                }

                foreach (var transition in _transitions)
                {
                    determined.AddTransition(transition);
                }
                
                determined.Init(_initialState.Id);
            }
            catch (NonDeterminedException e)
            {
                // Mathematical way for convert nondetermined state machine to determined
                throw new System.NotImplementedException();
            }

            return determined;
        }

        private List<State> EpsilonClosure(State state)
        {
            // var closure = new List<State> {state};
            // var buffer = _transitions
            //     .Where(tr => tr.StartState.Equals(state) && tr.IsEpsilon)
            //     .Select(tr => tr.EndState)
            //     .ToList();

            var closure = new List<State>();
            var buffer = new List<State> {state};
            
            while (buffer.Any())
            {
                var pickedState = buffer[0];
                buffer.Remove(pickedState);
                
                buffer.AddRange(_transitions
                    .Where(tr => tr.StartState.Equals(pickedState) && tr.IsEpsilon)
                    .Select(tr => tr.EndState));
                
                closure.Add(pickedState);
            }
            
            return closure;
        }
    }
}