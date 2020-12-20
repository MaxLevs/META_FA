using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Regex_Parser;
using Regex_Parser.CST;
using Regex_Parser.Visitors.AST;
using StateMachineLib.Options;
using StateMachineLib.Options.Exception;
using StateMachineLib.StateMachine;
using StateMachineLib.StateMachine.Exceptions;

namespace META_FA
{
    class Program
    {
        static string _optionsFilePath = "Examples/example_min_dfa_var.fa";
        private static string _regexpForParsing;
        private static string _outputPath;
        private static readonly List<Asset> Assets = new List<Asset>();
        
        private static bool _determ;
        private static bool _minimize;
        private static bool _printTable;
        private static bool _printDot;
        private static bool _dverbose;
        
        static void Main(string[] args)
        {
            while (args.Any())
            {
                var arg = args[0];
                args = args.Where((_, i) => i != 0).ToArray();

                if (arg.StartsWith("-"))
                {
                    switch (arg)
                    {
                        case "-r":
                        case "--regex":
                            _regexpForParsing = args[0];
                            args = args.Where((_, i) => i != 0).ToArray();
                            
                            break;
                        
                        case "-o":
                        case "--out":
                            var path = args[0];
                            args = args.Where((_, i) => i != 0).ToArray();
                            
                            if (Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute)
                                && Directory.Exists(path))
                            {
                                _outputPath = path;
                            }
                            break;
                        
                        case "-M":
                        case "--minimize":
                            _minimize = true;
                            break;
                        
                        case "-D":
                        case "--determine":
                            _determ = true;
                            _dverbose = false;
                            break;
                        
                        case "-D:verbose":
                        case "--determine:verbose":
                            _determ = true;
                            _dverbose = true;
                            break;
                        
                        
                        case "--table":
                            _printTable = true;
                            break;
                        
                        case "--dot":
                            _printDot = true;
                            break;
                        
                        case "--asset":
                        case "--asset:true":
                            Assets.Add(new Asset {Text = args[0], ExpectedResult = true});
                            args = args.Where((_, i) => i != 0).ToArray();
                            break;
                        
                        case "--asset:false":
                            Assets.Add(new Asset {Text = args[0], ExpectedResult = false});
                            args = args.Where((_, i) => i != 0).ToArray();
                            break;
                        
                        default:
                            break;
                    }
                }
                else
                {
                    _optionsFilePath = arg;
                }
            }
            
            try
            {
                _optionsFilePath = args[0];
            } catch (IndexOutOfRangeException) {}

            try
            {
                Machine stateMachine;

                if (string.IsNullOrEmpty(_regexpForParsing))
                {
                    Console.WriteLine($"[Info] Used options file is {_optionsFilePath}");
                    var options = Options.FromFile(_optionsFilePath)[0];
                    stateMachine = Machine.GetFromOptions(options.Arch);
                    if (options.Assets != null) Assets.AddRange(options.Assets);
                }

                else
                {
                    Console.WriteLine($"[Info] Generating state machine with regex \"{_regexpForParsing}\"");

                    var regexpParser = RegexpGrammar.GetParser();
                    var parseRes = regexpParser.Parse(_regexpForParsing);

                    // Console.WriteLine(parseRes.Dot()); return;

                    var cstBuilder = new CSTBuilderVisitor();
                    cstBuilder.Visit(parseRes);
                    var cst = (RegexCST) cstBuilder.GetResult();

                    // Console.WriteLine(cst.Dot()); return;

                    var stateMachineBuilder = new RegexStateMachineBuilderVisitor();
                    cst.Visit(stateMachineBuilder);

                    stateMachine = stateMachineBuilder.GetResult();

                    // Console.WriteLine(stateMachine.ToOptions().ToDot()); return;
                }

                Console.WriteLine($"[Info] Type: {stateMachine.Type}, MachineId: {stateMachine.Id}");
                Console.WriteLine();

                PrintTable(stateMachine);
                PrintDot(stateMachine);
                SaveMachineIntoFile(stateMachine, "NonDeterm");

                if (_determ && stateMachine.Type != MachineType.Determined)
                {

                    Console.WriteLine();
                    Console.WriteLine(new string('=', 60));
                    Console.WriteLine("[Action] Determining...");

                    stateMachine = stateMachine.Determine(_dverbose).RenameToNormalNames("s");
                    if (_dverbose)
                    {
                        Console.WriteLine(new string('=', 60));
                        Console.WriteLine("[Info] State machine was determined");
                    }
                    Console.WriteLine($"[Info] New id: {stateMachine.Id}");
                    Console.WriteLine();

                    PrintTable(stateMachine);
                    PrintDot(stateMachine);

                    SaveMachineIntoFile(stateMachine, "Determ");
                }

                if (_minimize)
                {
                    Console.WriteLine();
                    Console.WriteLine(new string('=', 60));
                    Console.WriteLine("[Action] Minimizing...");

                    stateMachine = stateMachine.Minimize().RenameToNormalNames("m");
                    Console.WriteLine($"[Info] New id: {stateMachine.Id}");
                    Console.WriteLine();

                    PrintTable(stateMachine);
                    PrintDot(stateMachine);
                    SaveMachineIntoFile(stateMachine, "MinDeterm");
                }
                
                TestAssets(Assets, stateMachine);
            }

            catch (FileNotFoundException)
            {
                Console.Error.WriteLine("[Error] Can't load configuration file");
            }

            catch (LoadFromFileException ex)
            {
                Console.Error.WriteLine("[Error] " + ex.Message);
            }

            catch (CoreSMException e)
            {
                Console.Error.WriteLine($"Some errors happened when machine was running:\n {e.Message}");
            }

        }

        private static void SaveMachineIntoFile(Machine stateMachine, string fileName)
        {
            if (!string.IsNullOrEmpty(_outputPath))
            {
                new Options {Arch = stateMachine.ToOptions(), Assets = Assets}
                    .ToFile(Path.Combine(_outputPath, $"{fileName}Output_{stateMachine.Id.ToString().Substring(0, 7)}.json"));
            }
        }

        private static void PrintDot(Machine stateMachine)
        {
            if (!_printDot) return;
            
            Console.WriteLine("Graph.dot");
            Console.WriteLine();
            Console.WriteLine(stateMachine.ToOptions().ToDot());
            Console.WriteLine();
        }

        private static void PrintTable(Machine stateMachine)
        {
            if(!_printTable) return;
            
            Console.WriteLine("Table");
            Console.WriteLine();
            Console.WriteLine(stateMachine.ToOptions().ToTable());
            Console.WriteLine();
        }

        private static void TestAssets(IEnumerable<Asset> assets, Machine stateMachine)
        {
            var assetsList = assets.ToList();
            
            if (assetsList.Any())
            {
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("[Action] Testing assets...");
                Console.WriteLine();
                
                var results = assetsList.ToDictionary(asset => asset, asset => stateMachine.Run(asset.Text) == asset.ExpectedResult);

                // foreach (var (text, expectedRes) in options.Assets)
                foreach (var (asset, result) in results)
                {
                    Console.Write(new string(' ', 3));
                    Console.Write($"Testing \"{asset.Text}\"… ".PadRight(results.Max(pair => pair.Key.Text?.Length ?? 4) + 13));
                    Console.Write($"Expected: {asset.ExpectedResult}. ".PadRight(19));
                    Console.Write("Result: "); 
                    
                    if (result)
                    {
                        Console.Write("Correct".PadRight(15));
                    }
                    
                    else
                    {
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Reject!".PadRight(15));
                        Console.ForegroundColor = color;
                    }
                    
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
