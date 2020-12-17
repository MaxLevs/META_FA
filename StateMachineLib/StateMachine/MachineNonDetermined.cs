using System;
using System.Collections.Generic;
using System.Linq;
using StateMachineLib.StateMachine.Exceptions;

namespace StateMachineLib.StateMachine
{
    public class MachineNonDetermined : Machine
    {
        public override MachineType Type => MachineType.NonDetermined;
        protected override void PreAddTransitionCheck(Transition newTransition)
        {
            var foundTransition = Transitions.Find(transition
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
            if (text.Length == 0)
                return currentState.IsFinal;
            
            var closure = EpsilonClosure(new List<State>{currentState});
            var token = text[0].ToString();
            
            return Transitions.Any(tr => closure.Contains(tr.StartState) && tr.Token == token && DoStep(text.Substring(1), tr.EndState));
        }
        
        public override Machine Minimize()
        {
            throw new NotImplementedException("Minimize() isn't implemented for Nondetermed state machine. Please call Determine() before");
        }

        public override MachineDetermined Determine()
        {
            var determined = new MachineDetermined();

            if (IsThereAnyEpsilonTransition || IsThereAnyMultiVariantTokenTransition)
            {
                var tokens = Transitions.Select(tr => tr.Token).Distinct().ToList();
                tokens.Remove(null);
                tokens.Sort();

                var initialClosure = EpsilonClosure(new List<State> {InitialState});
                var buffer = new List<List<State>> { initialClosure };
                var newStates = new List<List<State>>();

                Console.WriteLine("ε-closure({" + InitialState.Id + "}) = " + GetClosureName(initialClosure) + "\n");
                
                while (buffer.Any())
                {
                    var currentClosure = buffer[0];
                    buffer.Remove(currentClosure);
                    
                    newStates.Add(currentClosure);
                    
                    foreach (var token in tokens)
                    {
                        var nextStops = Transitions.Where(tr =>
                            !tr.IsEpsilon && currentClosure.Contains(tr.StartState) && tr.Token == token)
                            .Select(tr => tr.EndState)
                            .ToList();

                        var newClosure = EpsilonClosure(nextStops);
                        
                        // Console.WriteLine($"{GetClosureName(currentClosure)} ==[{token}]==> ε-closure({GetClosureName(nextStops)}) = {GetClosureName(newClosure)} ");
                        Console.Write( $"Move({GetClosureName(currentClosure)}, {token}) =  {GetClosureName(nextStops)};");

                        if (!newClosure.Any()
                            || newStates.Select(GetClosureName).Contains(GetClosureName(newClosure))
                            || buffer.Select(GetClosureName).Contains(GetClosureName(newClosure)))
                        {
                            Console.WriteLine();
                            continue;   
                        }
                        
                        Console.WriteLine($"     ε-closure({GetClosureName(nextStops)}) = {GetClosureName(newClosure)}");
                        
                        buffer.Add(newClosure);
                    }
                    
                    Console.WriteLine();
                }

                var determinedStates = newStates.Select(state => new State(GetClosureName(state), state.Any(x => x.IsFinal))).ToList();
                determined.AddStateRange(determinedStates);

                foreach (var startNewState in newStates)
                {
                    foreach (var token in tokens)
                    {
                        // Should I use Where() instead of Find()?
                        var way = Transitions.Find(tr => startNewState.Contains(tr.StartState) && tr.Token == token);
                        
                        if (way == null) continue;

                        // Should I use Where() instead of Find()?
                        var endNewState = newStates.Find(st => st.Contains(way.EndState));

                        var startDeterminedState = determinedStates.Find(st => st.Id == GetClosureName(startNewState));
                        var endDeterminedState = determinedStates.Find(st => st.Id == GetClosureName(endNewState));
                        
                        determined.AddTransition(new Transition(startDeterminedState, token, endDeterminedState));
                    }
                }

                determined.Init(GetClosureName(initialClosure));
            }

            else
            {
                determined.AddStateRange(States);
                determined.AddTransitionRange(Transitions);
                determined.Init(InitialState.Id);
            }

            return determined;
        }
        
        private List<State> EpsilonClosure(IEnumerable<State> states)
        {
            var closure = new List<State>();
            var buffer = new List<State>(states);
            
            while (buffer.Any())
            {
                var pickedState = buffer[0];
                buffer.Remove(pickedState);
                
                closure.Add(pickedState);

                var nextStates = Transitions
                    .Where(tr => tr.IsEpsilon && tr.StartState.Equals(pickedState) && !closure.Contains(tr.EndState) && !buffer.Contains(tr.EndState))
                    .Select(tr => tr.EndState)
                    .ToList();
                
                buffer.AddRange(nextStates);
            }

            closure.Sort((state1, state2) => string.CompareOrdinal(state1.Id, state2.Id));
            return closure;
        }
        
        private static string GetClosureName(List<State> closure)
        {
            return "{" + string.Join(",", closure) + "}";
        }
        
        public new MachineNonDetermined RenameToNormalNames()
        {
            var renameDict = States
                .Select((state, n) => new {NewState = new State($"q{n+1}", state.IsFinal), OldState = state})
                .ToDictionary(x => x.OldState, x => x.NewState);
            
            var renamedMachine = new MachineNonDetermined();
            renamedMachine.AddStateRange(renameDict.Values);
            renamedMachine.AddTransitionRange(Transitions.Select(transition => transition.IsEpsilon? new Transition(renameDict[transition.StartState], renameDict[transition.EndState]) : new Transition(renameDict[transition.StartState], transition.Token, renameDict[transition.EndState])));
            renamedMachine.Init(renameDict[InitialState].Id);

            return renamedMachine;
        }
    }
}