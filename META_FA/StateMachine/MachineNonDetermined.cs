using META_FA.StateMachine.Exceptions;

namespace META_FA.StateMachine
{
    public class MachineNonDetermined : Machine
    {
        protected override MachineType Type => MachineType.NonDetermined;
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
            throw new System.NotImplementedException();
        }
    }
}