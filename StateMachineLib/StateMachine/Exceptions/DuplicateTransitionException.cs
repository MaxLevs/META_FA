namespace StateMachineLib.StateMachine.Exceptions
{
    public class DuplicateTransitionException: CoreSMException
    {
        public override string Message { get; }

        public DuplicateTransitionException(Transition transition, Machine machine)
        {
            Message = $"There is another transitions with this way";
        }
    }
}