using System;
using META_FA.Assets;
using META_FA.Options;
using META_FA.StateMachine;

namespace META_FA
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = SMOptions.FromFile("machine_arch.json");
            var assets = Asset.FromFile("assets.json");
            
            var stateMachine = Machine.GetFromOptions(options);
            
            Console.WriteLine($"Type: {stateMachine.Type}, MachineId: {stateMachine.Id}");
            Console.WriteLine();
            
            foreach (var (text, expectedRes) in assets)
            {
                Console.Write($"Test \"{text}\". ");
                Console.Write($"Expected: {expectedRes}. ");
                Console.Write($"Result: {(stateMachine.Run(text) == expectedRes ? "Correct" : "Reject!")}");
                Console.WriteLine();
            }
            
            Console.WriteLine();
            Console.WriteLine(stateMachine.ToOptions().ToTable());
            
            Console.WriteLine();
            Console.WriteLine(stateMachine.ToOptions().ToDot());
        }
    }
}
