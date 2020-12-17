namespace StateMachineLib.StateMachine.Exceptions
{
    public class NonDeterminedException : CoreSMException
    {
        public override string Message { get; }
        
        public NonDeterminedException(Transition transition, Machine machine)
        {
            Message = $"Nondetermined transition detected. Machine: {machine.Id}. Transition: {transition.Id}";
        }
    }
}