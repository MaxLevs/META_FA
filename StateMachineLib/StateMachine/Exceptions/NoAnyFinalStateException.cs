namespace StateMachineLib.StateMachine.Exceptions
{
    public class NoAnyFinalStateException : CoreSMException
    {
        public override string Message { get; }

        public NoAnyFinalStateException(Machine machine)
        {
            Message = $"There is no any final state in this machine: {machine.Id}";
        }
    }
}