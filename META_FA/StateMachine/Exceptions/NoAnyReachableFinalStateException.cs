namespace META_FA.StateMachine.Exceptions
{
    public class NoAnyReachableFinalStateException : CoreSMException
    {
        public override string Message { get; }

        public NoAnyReachableFinalStateException(StateMachine machine)
        {
            Message = $"There are no any reachable final states into machine: {machine.Id}";
        }
    }
}