namespace StateMachineLib.StateMachine.Exceptions
{
    public class DuplicateStateException : CoreSMException
    {
        public override string Message { get; }

        public DuplicateStateException(State state, Machine machine)
        {
            Message = $"There almost is another state with the same name in this state machine: {machine.Id}[{state.Id}]";
        }
    }
}