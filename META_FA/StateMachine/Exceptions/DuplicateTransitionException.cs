namespace META_FA.StateMachine.Exceptions
{
    public class DuplicateTransitionException: CoreSMException
    {
        public override string Message { get; }

        public DuplicateTransitionException(Transition transition, StateMachine machine)
        {
            Message = $"There is another transitions with this way";
        }
    }
}