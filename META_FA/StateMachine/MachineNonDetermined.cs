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

            bool isThereAnyEpsilonTransition = _transitions.Any(transition => transition.IsEpsilon);
            bool isThereAnyMultiVariantTokenTransition = _transitions
                .GroupBy(tr => new {tr.StartState, tr.Token})
                .Any(gr => gr.Count() > 1);

            if (isThereAnyEpsilonTransition || isThereAnyMultiVariantTokenTransition)
            {
                var tokens = _transitions.Select(tr => tr.Token).Distinct().ToList();
                tokens.Sort();
                
                var initialClosure = EpsilonClosure(_initialState);
                var buffer = new List<List<State>> { initialClosure };
                var newStates = new List<List<State>> {};

                while (buffer.Any())
                {
                    var currentClosure = buffer[0];
                    buffer.Remove(currentClosure);
                    
                    foreach (var token in tokens)
                    {
                        var newClosures = _transitions
                            .Where(tr => !tr.IsEpsilon && currentClosure.Contains(tr.StartState) && tr.Token == token)
                            .Select(tr => EpsilonClosure(tr.EndState));
                        
                        buffer.AddRange(newClosures);
                    }
                    
                    newStates.Add(currentClosure);
                }

                // todo: add realisation
            }

            else
            {
                determined.AddStateRange(_states);
                determined.AddTransitionRange(_transitions);
                determined.Init(_initialState.Id);
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

                var nextStates = _transitions
                    .Where(tr => tr.IsEpsilon && tr.StartState.Equals(pickedState))
                    .Select(tr => tr.EndState);
                
                buffer.AddRange(nextStates);
                
                closure.Add(pickedState);
            }
            
            return closure;
        }
    }
}