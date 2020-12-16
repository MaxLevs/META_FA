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
                List<IGrouping<string, State>> newSplitting = new List<IGrouping<string, State>>();
                
                foreach (var currentCategory in currentSplitting)
                {
                    var subSplittingIntoCurrentCategory = currentCategory.GroupBy(checkedStartState => {
                        var kLocal = k;
                        var currentSplittingLocal = currentSplitting;
                        
                        var movementsRow = string.Join("|", tokens.Select(checkedToken =>
                        {
                            var endState = _transitions.Find(transition =>
                                Equals(transition.StartState, checkedStartState) && transition.Token == checkedToken)?.EndState;

                            if (endState == null)
                            {
                                return "";
                            }

                            var endCategory = currentSplittingLocal.Find(suspectedCategory => suspectedCategory.Contains(endState));
                            var nameOfEndCategory = "{" + string.Join(",", endCategory) + "}[" + kLocal + "]";
                            
                            return nameOfEndCategory;
                        }));

                        return movementsRow;
                    }).ToList();
                    
                    newSplitting.AddRange(subSplittingIntoCurrentCategory);
                }

                currentSplitting = newSplitting;
                
                // If current splitting ans new splitting are equal 
                // Then break; here [todo]
            }
            
            // TODO:
            // - Create States from categories
            // - Create Transitions with this states
            
            throw new System.NotImplementedException();
        }

        public override MachineDetermined Determine()
        {
            return this;
        }
    }
}