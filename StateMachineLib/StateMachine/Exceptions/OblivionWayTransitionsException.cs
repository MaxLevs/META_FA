namespace StateMachineLib.StateMachine.Exceptions
{
    public class OblivionWayTransitionsException : CoreSMException
    {
        public override string Message { get; }

        public OblivionWayTransitionsException(Machine machine)
        {
            Message = $"There are oblivion way transitions into machine: {machine.Id}";
        }
    }
}