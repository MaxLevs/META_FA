using System;
using System.Collections.Generic;
using System.Linq;
using StateMachineLib.StateMachine.Exceptions;

namespace StateMachineLib.StateMachine
{
    public class MachineDetermined : Machine
    {
        public override MachineType Type => MachineType.Determined;
        
        protected override void PreAddTransitionCheck(Transition newTransition)
        {
            var foundTransition = Transitions.Find(transition
                => Equals(transition.StartState, newTransition.StartState)
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
            
            var token = text[0].ToString();
            var way = Transitions.Find(transition => Equals(currentState, transition.StartState) && token == transition.Token);

            return way != null && DoStep(text.Substring(1), way.EndState);
        }

        public override Machine Minimize()
        {
            var tokens = Transitions.Select(transition => transition.Token).Distinct().ToList();
            tokens.Sort();

            var currentSplitting = States.GroupBy(state => state.IsFinal ? "A0" : "B0").ToList();

            for (int k = 1; k <= States.Count; ++k)
            {
                var newSplitting = new List<IGrouping<string, State>>();
                
                foreach (var currentCategory in currentSplitting)
                {
                    var subSplittingIntoCurrentCategory = currentCategory.GroupBy(checkedStartState => {
                        var kLocal = k;
                        var currentSplittingLocal = currentSplitting;

                        var movementCategories = tokens.Select(checkedToken =>
                        {
                            var endState = Transitions.Find(transition =>
                                    Equals(transition.StartState, checkedStartState) &&
                                    transition.Token == checkedToken)
                                ?.EndState;

                            if (endState == null)
                            {
                                return "";
                            }

                            var endCategory = currentSplittingLocal.Find(suspectedCategory =>
                                suspectedCategory.Contains(endState));
                            
                            var nameOfEndCategory = "{" + string.Join(",", endCategory) + "}" /* + "[" + kLocal + "]"*/;
                            return nameOfEndCategory;
                        });
                        
                        var movementsRow = string.Join("|", movementCategories);
                        
                        return movementsRow;
                    }).ToList();
                    
                    newSplitting.AddRange(subSplittingIntoCurrentCategory);
                }

                currentSplitting = newSplitting;
                
                // If current splitting ans new splitting are equal 
                // Then break; here [todo]
            }
            
            var minimizedStateMachine = new MachineDetermined();
            
            var newStates = currentSplitting.Select(category => new { State = new State("{" + string.Join(",",category.Select(state => state.Id)) + "}", category.Any(state => state.IsFinal)), Category = category}).ToList();

            minimizedStateMachine.AddStateRange(newStates.Select(state => state.State));
            
            foreach (var startPoint in newStates)
            {
                var ways = startPoint.Category.Key.Split("|")
                    .Zip(tokens, (endPoint, token) => new {Token = token, EndPoint = endPoint})
                    .Where(x => !string.IsNullOrEmpty(x.EndPoint))
                    .Select(x =>
                    {
                        var endPointState = newStates.Find(state => state.State.Id == x.EndPoint);
                        
                        return new Transition(startPoint.State, x.Token, endPointState.State); // Null check??
                    });
                
                minimizedStateMachine.AddTransitionRange(ways);
            }
            
            minimizedStateMachine.Init(newStates.Find(x => x.Category.Contains(InitialState))?.State.Id ?? throw new InitialStateIsNullException("null", minimizedStateMachine));

            return minimizedStateMachine;
        }

        public override MachineDetermined Determine(bool verbose = false)
        {
            if (verbose) Console.WriteLine($"[Info] Machine {Id} is already determined");
            return this;
        }

        public new MachineDetermined RenameToNormalNames(string startsWith)
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
            
            var renamedMachine = new MachineDetermined();
            renamedMachine.AddStateRange(renameDict.Values);
            renamedMachine.AddTransitionRange(Transitions.Select(transition => new Transition(renameDict[transition.StartState], transition.Token, renameDict[transition.EndState])));
            renamedMachine.Init(renameDict[InitialState].Id);

            return renamedMachine;
        }
    }
}