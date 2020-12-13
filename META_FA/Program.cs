using System;
using META_FA.StateMachine;

namespace META_FA
{
    class Program
    {
        static void Main(string[] args)
        {
            var stateMachine = new StateMachine.StateMachine();
            var state1 = new State("A", false);
            var state2 = new State("B", true);
            var transition = new Transition("[", state1, state2);
            stateMachine.AddState(state1);
            stateMachine.AddState(state2);
            stateMachine.AddTransition(transition);
            stateMachine.Init("A");
        }
    }
}