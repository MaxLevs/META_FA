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
            var closure = EpsilonClosure(new List<State>{currentState});

            if (text.Length == 0)
                return closure.Any(st => st.IsFinal);
            
            var token = text[0].ToString();

            var suspectedEndStates = Transitions.Where(tr => closure.Contains(tr.StartState) && tr.Token == token)
                .Select(tr => tr.EndState).ToList();

            var result = suspectedEndStates.Any(nextState => DoStep(text.Substring(1), nextState));
            return result;
        }
        
        public override Machine Minimize()
        {
            throw new NotImplementedException("Minimize() isn't implemented for Nondetermed state machine. Please call Determine() before");
        }

        private class TempTransition
        {
            public string Q0 { get; }
            public string Token { get; }
            public string Q1 { get; }

            public TempTransition(string q0, string token, string q1)
            {
                Q0 = q0;
                Token = token;
                Q1 = q1;
            }
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
                var tempMovements = new List<TempTransition>();

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
                        
                        Console.Write( $"Move({GetClosureName(currentClosure)}, {token}) =  {GetClosureName(nextStops)};");
                        
                        if (newClosure.Any())
                        {
                            tempMovements.Add(new TempTransition(GetClosureName(currentClosure), token, GetClosureName(newClosure)));
                        }

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

                foreach (var startNewState in determinedStates)
                {
                    foreach (var token in tokens)
                    {
                        var way = tempMovements.Find(mv => mv.Q0 == startNewState.Id && mv.Token == token);
                        
                        if (way == null)
                            continue;

                        var nextStop = determinedStates.Find(st => st.Id == way.Q1);
                        
                        determined.AddTransition(new Transition(startNewState, token, nextStop));
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
                    .Where(tr => 
                                    tr.IsEpsilon 
                                 && tr.StartState.Id == pickedState.Id
                                 && !closure.Contains(tr.EndState)
                                 && !buffer.Contains(tr.EndState))
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
        
        public new MachineNonDetermined RenameToNormalNames(string startsWith = null)
        {
            startsWith ??= "q";
            
            var renameDict = new Dictionary<State, State>();

            var buffer = new List<State> { InitialState };

            var n = 1;
            while (buffer.Any())
            {
                var currentNode = buffer[0];

                var nextStops = Transitions.Where(tr => Equals(tr.StartState, currentNode) && !buffer.Contains(tr.EndState) && !renameDict.Keys.Contains(tr.EndState)).Select(tr=>tr.EndState).Distinct(new StatesComparer()).ToList();

                if (nextStops.Any())
                {
                    buffer.AddRange(nextStops);
                }
                
                buffer.Remove(currentNode);
                renameDict.Add(currentNode, new State($"{startsWith}{n}", currentNode.IsFinal));
                n++;
            }
            
            var renamedMachine = new MachineNonDetermined();
            renamedMachine.AddStateRange(renameDict.Values);
            renamedMachine.AddTransitionRange(Transitions.Select(transition => transition.ChangeStartState(renameDict[transition.StartState]).ChangeEndState(renameDict[transition.EndState])));
            renamedMachine.Init(renameDict[InitialState].Id);

            return renamedMachine;
        }
    }
}