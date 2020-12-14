using System;
using System.Runtime.CompilerServices;

namespace META_FA.StateMachine.Exceptions
{
    public class InitialStateIsNullException : CoreSMException
    {
        public override string Message { get; }

        public InitialStateIsNullException(string stateId, Machine machine)
        { 
            Message = $"Cannot found initial state by id: {stateId} [machine: {machine.Id}]";
        }
    }
}