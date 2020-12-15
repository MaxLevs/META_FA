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
    }
}