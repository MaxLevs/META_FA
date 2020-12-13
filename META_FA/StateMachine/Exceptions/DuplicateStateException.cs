namespace META_FA.StateMachine.Exceptions
{
    public class DuplicateStateException : CoreSMException
    {
        public override string Message { get; }

        public DuplicateStateException(State state, StateMachine stateMachine)
        {
            Message = $"There almost is another state with the same name in this state machine: {stateMachine.Id}[{state.Id}]";
        }
    }
}