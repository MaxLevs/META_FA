namespace META_FA.StateMachine.Exceptions
{
    public class UninitedMachineRunException : CoreSMException
    {
        public override string Message { get; }

        public UninitedMachineRunException(Machine machine)
        {
            Message = $"This machine was not inited. Call Init() before Run(). Machine: {machine.Id}";
        }
    }
}