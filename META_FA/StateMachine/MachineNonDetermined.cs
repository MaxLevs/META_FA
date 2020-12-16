using System;
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
                tokens.Remove(null);
                tokens.Sort();
                
                var initialClosure = EpsilonClosure(_initialState);
                var buffer = new List<List<State>> { initialClosure };
                var newStates = new List<List<State>> {};

                while (buffer.Any())
                {
                    var currentClosure = buffer[0];
                    buffer.Remove(currentClosure);
                    
                    newStates.Add(currentClosure);
                    
                    foreach (var token in tokens)
                    {
                        var newClosures = _transitions
                            .Where(tr => !tr.IsEpsilon && currentClosure.Contains(tr.StartState) && tr.Token == token)
                            .Select(tr => EpsilonClosure(tr.EndState))
                            .Where(closure =>
                                !newStates.Select(ClosureComparer.GetClosureName).Contains(ClosureComparer.GetClosureName(closure))
                                && !buffer.Select(ClosureComparer.GetClosureName).Contains(ClosureComparer.GetClosureName(closure)))
                            .ToList();
                        
                        buffer.AddRange(newClosures);
                    }
                }

                var determinedStates = newStates.Select(state => new State(ClosureComparer.GetClosureName(state), state.Any(x => x.IsFinal))).ToList();
                determined.AddStateRange(determinedStates);

                foreach (var startNewState in newStates)
                {
                    foreach (var token in tokens)
                    {
                        // Should I use Where() instead of Find()?
                        var way = _transitions.Find(tr => startNewState.Contains(tr.StartState) && tr.Token == token);
                        
                        if (way == null) continue;

                        // Should I use Where() instead of Find()?
                        var endNewState = newStates.Find(st => st.Contains(way.EndState));

                        var startDeterminedState = determinedStates.Find(st => st.Id == ClosureComparer.GetClosureName(startNewState));
                        var endDeterminedState = determinedStates.Find(st => st.Id == ClosureComparer.GetClosureName(endNewState));
                        
                        determined.AddTransition(new Transition(startDeterminedState, token, endDeterminedState));
                    }
                }

                determined.Init(ClosureComparer.GetClosureName(initialClosure));
            }

            else
            {
                determined.AddStateRange(_states);
                determined.AddTransitionRange(_transitions);
                determined.Init(_initialState.Id);
            }

            return determined;
        }

        internal class ClosureComparer : IEqualityComparer<List<State>>
        {
            public static string GetClosureName(List<State> closure)
            {
                return "{" + string.Join(",", closure) + "}";
            }
            
            public bool Equals(List<State> x, List<State> y)
            {
                return GetClosureName(x) == GetClosureName(y);
            }

            public int GetHashCode(List<State> obj)
            {
                return HashCode.Combine(GetClosureName(obj));
            }
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
                
                closure.Add(pickedState);

                var nextStates = _transitions
                    .Where(tr => tr.IsEpsilon && tr.StartState.Equals(pickedState))
                    .Select(tr => tr.EndState)
                    .ToList();
                
                buffer.AddRange(nextStates);
            }

            closure.Sort((state1, state2) => string.CompareOrdinal(state1.Id, state2.Id));
            return closure;
        }
    }
}