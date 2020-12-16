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
            var tokens = _transitions.Select(transition => transition.Token).Distinct().ToList();
            tokens.Sort();

            var currentSplitting = _states.GroupBy(state => state.IsFinal ? "A0" : "B0").ToList();

            for (int k = 1; k <= _states.Count; ++k)
            {
                var newSplitting = new List<IGrouping<string, State>>();
                
                foreach (var currentCategory in currentSplitting)
                {
                    var subSplittingIntoCurrentCategory = currentCategory.GroupBy(checkedStartState => {
                        var kLocal = k;
                        var currentSplittingLocal = currentSplitting;

                        var movementCategories = tokens.Select(checkedToken =>
                        {
                            var endState = _transitions.Find(transition =>
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
            var renameDict = newStates
                .Select(x => x.State.Id)
                .Zip(newStates.Select((_, i) => (i + 1).ToString()), (k, v) => new {Key = k, Value = v})
                .ToDictionary(data => data.Key, data => data.Value);

            var renamedStates = newStates.Select(x => new State(renameDict[x.State.Id], x.State.IsFinal)).ToList();
            minimizedStateMachine.AddStateRange(renamedStates);
            
            foreach (var startPoint in newStates)
            {
                var ways = startPoint.Category.Key.Split("|")
                    .Zip(tokens, (endPoint, token) => new {Token = token, EndPoint = endPoint})
                    .Where(x => !string.IsNullOrEmpty(x.EndPoint))
                    .Select(x =>
                    {
                        var endPointState = newStates.Find(state => state.State.Id == x.EndPoint);
                        var renamedStartState = renamedStates.Find(state => state.Id == renameDict[startPoint.State.Id]);
                        var renamedEndState = renamedStates.Find(state => state.Id == renameDict[endPointState.State.Id]);
                        
                        return new Transition(renamedStartState, x.Token, renamedEndState); // Null check??
                    });
                
                minimizedStateMachine.AddTransitionRange(ways);
            }
            
            minimizedStateMachine.Init(renameDict[newStates.Find(x => x.Category.Contains(_initialState))?.State.Id ?? throw new InitialStateIsNullException("null", minimizedStateMachine)]);

            return minimizedStateMachine;
        }

        public override MachineDetermined Determine()
        {
            return this;
        }
    }
}