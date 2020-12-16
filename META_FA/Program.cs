using System;
using System.IO;
using META_FA.Assets;
using META_FA.Options;
using META_FA.StateMachine;
using META_FA.StateMachine.Exceptions;

namespace META_FA
{
    class Program
    {
        static void Main(string[] args)
        {
            string optionsFilePath = "machine_arch.json";
            try
            {
                optionsFilePath = args[1];
            } catch (IndexOutOfRangeException) {}

            try
            {
                Console.WriteLine($"[Info] Used arch file is {optionsFilePath}");
                var options = SMOptions.FromFile(optionsFilePath);
                var stateMachine = Machine.GetFromOptions(options);

                Console.WriteLine($"[Info] Type: {stateMachine.Type}, MachineId: {stateMachine.Id}");
                Console.WriteLine();

                // var assets = Asset.FromFile("assets.json");
                // foreach (var (text, expectedRes) in assets)
                // {
                //     Console.Write($"Test \"{text}\". ");
                //     Console.Write($"Expected: {expectedRes}. ");
                //     Console.Write($"Result: {(stateMachine.Run(text) == expectedRes ? "Correct" : "Reject!")}");
                //     Console.WriteLine();
                // }
                // Console.WriteLine();

                Console.WriteLine(stateMachine.ToOptions().ToTable());
                Console.WriteLine();

                Console.WriteLine(stateMachine.ToOptions().ToDot());
                Console.WriteLine();

                stateMachine = stateMachine.Minimize();
                Console.WriteLine(stateMachine.ToOptions().ToDot());
            }

            catch (FileNotFoundException)
            {
                Console.Error.WriteLine("[Error] Can't load configuration file");
            }

            catch (CoreSMException e)
            {
                Console.Error.WriteLine($"Some errors happened when machine was running:\n {e.Message}");
            }

        }
    }
}
