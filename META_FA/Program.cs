using System;
using System.IO;
using System.Linq;
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
                optionsFilePath = args[0];
            } catch (IndexOutOfRangeException) {}

            try
            {
                Console.WriteLine($"[Info] Used options file is {optionsFilePath}");
                var options = Options.Options.FromFile(optionsFilePath);
                var stateMachine = Machine.GetFromOptions(options.Arch);

                Console.WriteLine($"[Info] Type: {stateMachine.Type}, MachineId: {stateMachine.Id}");
                Console.WriteLine();

                PrintTable(stateMachine);
                PrintDot(stateMachine);
                TestAssets(options, stateMachine);

                if (stateMachine.Type != MachineType.Determined)
                {
                    Console.WriteLine();
                    Console.WriteLine(new string('=', 60));
                    Console.WriteLine("[Action] Determining...");

                    stateMachine = stateMachine.Determine().RenameToNormalNames();
                    Console.WriteLine($"  [Info] New id: {stateMachine.Id}");
                    Console.WriteLine();
                    
                    PrintTable(stateMachine);
                    PrintDot(stateMachine);
                }

                Console.WriteLine();
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("[Action] Minimizing...");

                stateMachine = stateMachine.Minimize().RenameToNormalNames();
                Console.WriteLine($"  [Info] New id: {stateMachine.Id}");
                Console.WriteLine();
                
                PrintTable(stateMachine);
                PrintDot(stateMachine);
                TestAssets(options, stateMachine);
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

        private static void PrintDot(Machine stateMachine)
        {
            Console.WriteLine("Graph.dot");
            Console.WriteLine();
            Console.WriteLine(stateMachine.ToOptions().ToDot());
            Console.WriteLine();
        }

        private static void PrintTable(Machine stateMachine)
        {
            Console.WriteLine("Table");
            Console.WriteLine();
            Console.WriteLine(stateMachine.ToOptions().ToTable());
            Console.WriteLine();
        }

        private static void TestAssets(Options.Options options, Machine stateMachine)
        {
            if (options.Assets.Any())
            {
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("[Action] Testing assets...");
                Console.WriteLine();

                // foreach (var (text, expectedRes) in options.Assets)
                foreach (var asset in options.Assets)
                {
                    Console.Write(new string(' ', 3));
                    Console.Write($"Testing \"{asset.Text}\"… ".PadRight(options.Assets.Max(x => x.Text?.Length ?? 4) + 13));
                    Console.Write($"Expected: {asset.ExpectedResult}. ".PadRight(19));
                    Console.Write($"Result: {(stateMachine.Run(asset.Text) == asset.ExpectedResult ? "Correct" : "Reject!")}"
                        .PadRight(15));
                    Console.WriteLine();
                }

                Console.WriteLine();
            }

            else
            {
                Console.WriteLine("[Info] Test assets are not found");
            }
        }
    }
}
