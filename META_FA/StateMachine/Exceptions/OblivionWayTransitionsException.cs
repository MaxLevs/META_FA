namespace META_FA.StateMachine.Exceptions
{
    public class OblivionWayTransitionsException : CoreSMException
    {
        public override string Message { get; }

        public OblivionWayTransitionsException(StateMachine machine)
        {
            Message = $"There are oblivion way transitions into machine: {machine.Id}";
        }
    }
}