using System;
using System.Linq;
using META_FA.StateMachine.Exceptions;

namespace META_FA.StateMachine
{
    public class MachineDetermined : Machine
    {
        protected override MachineType Type => MachineType.Determined;
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
        
        private bool EquivalentK(State stateOne, State stateTwo, int k)
        {
            if (k < 0) throw new ArgumentException("Argument k can't be less than 0", nameof(k));

            if (k == 0)
            {
                return stateOne.IsFinal == stateTwo.IsFinal;
            }

            var nextStatesOne = _transitions.Where(transition => Equals(transition.StartState, stateOne)).Select(transition => transition.EndState).ToList();
            var nextStatesTwo = _transitions.Where(transition => Equals(transition.StartState, stateTwo)).Select(transition => transition.EndState).ToList();

            if (!EquivalentK(stateOne, stateTwo, k - 1))
            {
                return false;
            }

            return !nextStatesOne
                .SelectMany(nextStateOne => nextStatesTwo,
                    (nextStateOne, nextStateTwo) => new {nextStateOne, nextStateTwo})
                .Where(@t => !EquivalentK(@t.nextStateOne, @t.nextStateTwo, k - 1))
                .Select(@t => @t.nextStateOne).Any();
        }
        
        public override Machine Minimize()
        {
            throw new System.NotImplementedException();
        }

        public override MachineDetermined Determine()
        {
            return this;
        }
    }
}