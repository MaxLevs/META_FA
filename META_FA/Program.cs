using System;
using System.IO;
using META_FA.Options;
using META_FA.StateMachine;
using META_FA.StateMachine.Exceptions;

namespace META_FA
{
    class Program
    {
        static void Main(string[] args)
        {
            string optionsFilePath = "Examples/options.json";
            try
            {
                optionsFilePath = args[1];
            } catch (IndexOutOfRangeException) {}

            try
            {
                Console.WriteLine($"[Info] Used arch file is {optionsFilePath}");
                var options = Options.Options.FromFile(optionsFilePath);
                var stateMachine = Machine.GetFromOptions(options.Arch);

                Console.WriteLine($"[Info] Type: {stateMachine.Type}, MachineId: {stateMachine.Id}");
                Console.WriteLine();

                Console.WriteLine(stateMachine.ToOptions().ToTable());
                Console.WriteLine();

                Console.WriteLine(stateMachine.ToOptions().ToDot());
                Console.WriteLine();

                if (stateMachine.Type != MachineType.Determined)
                {
                    Console.WriteLine("[Action] Determining...");
                    Console.WriteLine();

                    stateMachine = stateMachine.Determine().RenameToNormalNames();
                    
                    Console.WriteLine(stateMachine.ToOptions().ToTable());
                    Console.WriteLine();
                    
                    Console.WriteLine(stateMachine.ToOptions().ToDot());
                    Console.WriteLine();
                }

                Console.WriteLine("[Action] Minimizing...");
                Console.WriteLine();

                stateMachine = stateMachine.Minimize().RenameToNormalNames();
                
                Console.WriteLine(stateMachine.ToOptions().ToTable());
                Console.WriteLine();
                
                Console.WriteLine(stateMachine.ToOptions().ToDot());
                Console.WriteLine();

                // foreach (var (text, expectedRes) in options.Assets)
                
                Console.WriteLine("[Action] Testing assets...");
                foreach (var asset in options.Assets)
                {
                    Console.Write($"Test \"{asset.Text}\". ");
                    Console.Write($"Expected: {asset.ExpectedResult}. ");
                    Console.Write($"Result: {(stateMachine.Run(asset.Text) == asset.ExpectedResult ? "Correct" : "Reject!")}");
                    Console.WriteLine();
                }
                Console.WriteLine();
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
