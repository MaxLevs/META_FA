using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DSL_Parser;
using DSL_Parser.CST;
using DSL_Parser.Visitors.AST;
using Regex_Parser;
using Regex_Parser.CST;
using Regex_Parser.Visitors.AST;
using StateMachineLib;
using StateMachineLib.Options;
using StateMachineLib.Options.Exception;
using StateMachineLib.StateMachine;
using StateMachineLib.StateMachine.Exceptions;

namespace META_FA
{
    class Program
    {
        static string _optionsFilePath = "Examples/future_example_min.fa";
        private static string _regexpForParsing;
        private static string _outputPath;
        private static readonly Dictionary<string, Machine> Machines = new Dictionary<string, Machine>();
        private static readonly List<Asset> Assets = new List<Asset>();
        
        private static readonly Dictionary<string, Action<IList<CstFuncArg>>> BuiltIn = new Dictionary<string, Action<IList<CstFuncArg>>>
        {
            {"Asset", args =>
            {
                if (args.Count != 3
                 || args[0].Type != DSLGrammar.Identity
                 || args[1].Type != DSLGrammar.String
                 || args[2].Type != DSLGrammar.Bool)
                    throw new NotImplementedException();
                
                var machine = Machines[((CstIdentity) args[0].Data).Name];
                var text = ((CstString) args[1].Data).Data;
                var expected = ((CstBool) args[2].Data).Data;
                
                TestAsset(machine, text, expected);
            }},
            
            {"Print", args =>
            {
                if (args.Count != 1
                 || args[0].Type != DSLGrammar.String)
                    throw new NotImplementedException();
                
                Console.WriteLine(((CstString) args[0].Data).Data);
            }},
            
            {"PrintInfo", args =>
            {
                if (args.Count != 1
                 || args[0].Type != DSLGrammar.Identity)
                    throw new NotImplementedException();
                
                var machine = Machines[((CstIdentity) args[0].Data).Name];
                
                PrintInfo(machine);
            }},
            
            {"PrintTable", args =>
            {
                if (args.Count != 1
                 || args[0].Type != DSLGrammar.Identity)
                    throw new NotImplementedException();
                
                var machine = Machines[((CstIdentity) args[0].Data).Name];
                
                PrintTable(machine);
            }},
            
            {"PrintDot", args =>
            {
                if (args.Count != 1
                 || args[0].Type != DSLGrammar.Identity)
                    throw new NotImplementedException();
                
                var machine = Machines[((CstIdentity) args[0].Data).Name];
                
                PrintDot(machine);
            }},
            
            {"Determine", args =>
            {
                if (args.Count != 2
                 || args[0].Type != DSLGrammar.Identity
                 || args[1].Type != DSLGrammar.Identity)
                    throw new NotImplementedException();
                
                var machine = Machines[((CstIdentity) args[0].Data).Name];
                var newName = ((CstIdentity) args[1].Data).Name;
                var determMachine = machine.Determine(_dverbose).RenameToNormalNames("q");
                Machines[newName] = determMachine;
            }},
            
            {"Minimize", args =>
            {
                if (args.Count != 2
                 || args[0].Type != DSLGrammar.Identity
                 || args[1].Type != DSLGrammar.Identity)
                    throw new NotImplementedException();
                
                var machine = Machines[((CstIdentity) args[0].Data).Name];
                var newName = ((CstIdentity) args[1].Data).Name;
                var minMachine = machine.Minimize().RenameToNormalNames("m");
                Machines[newName] = minMachine;
            }},
        };
        
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
                            throw new NotImplementedException();
                            // Assets.Add(new Asset {Text = args[0], ExpectedResult = true});
                            // args = args.Where((_, i) => i != 0).ToArray();
                            break;
                        
                        case "--asset:false":
                            throw new NotImplementedException();
                            // Assets.Add(new Asset {Text = args[0], ExpectedResult = false});
                            // args = args.Where((_, i) => i != 0).ToArray();
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
                if (string.IsNullOrEmpty(_regexpForParsing))
                {
                    Console.WriteLine($"[Info] Used options file is {_optionsFilePath}");
                    using var file = File.OpenText(_optionsFilePath);
                    var text = file.ReadToEnd();
                    file.Close();

                    var grammar = DSLGrammar.Build();
                    var ast = grammar.Parse(text);

                    if (ast == null) throw new LoadFromFileException(grammar, grammar.Goal, text);

                    var cstBuilder = new CstBuilderVisitor();
                    cstBuilder.Apply(ast);
                    var cst = (CstDsl) cstBuilder.GetResult();

                    var optionsBuilder = new DslStateMachineOptionsBuilderVisitor();
                    cst.Visit(optionsBuilder);

                    var options = (Dictionary<string, SMOptions>) optionsBuilder.GetResult();

                    foreach (var (machineName, machineOptions) in options)
                    {
                        Machines.Add(machineName, Machine.GetFromOptions(machineOptions));
                    }

                    foreach (var entity in cst.CodeEntities)
                    {
                        if (entity is CstFunctionCall functionCall)
                        {
                            BuiltIn[functionCall.FunctionName.Name](functionCall.Args);
                        }
                    }
                }

                else
                {
                    Console.WriteLine($"[Info] Generating state machine with regex \"{_regexpForParsing}\"");

                    var regexpParser = RegexpGrammar.GetParser();
                    var parseRes = regexpParser.Parse(_regexpForParsing);

                    // Console.WriteLine(parseRes.Dot()); return;

                    var cstBuilder = new CSTBuilderVisitor();
                    cstBuilder.Apply(parseRes);
                    var cst = cstBuilder.GetResult();

                    // Console.WriteLine(cst.Dot()); return;

                    var stateMachineBuilder = new RegexStateMachineBuilderVisitor();
                    cst.Visit(stateMachineBuilder);

                    Machines.Add("DEFAULT_MACHINE_REGEX", stateMachineBuilder.GetResult());

                    // Console.WriteLine(stateMachine.ToOptions().ToDot()); return;
                }

                //     Console.WriteLine($"[Info] Type: {stateMachine.Type}, MachineId: {stateMachine.Id}");
                //     Console.WriteLine();
                //
                //     PrintTable(stateMachine);
                //     PrintDot(stateMachine);
                //     SaveMachineIntoFile(stateMachine, "NonDeterm");
                //
                //     if (_determ && stateMachine.Type != MachineType.Determined)
                //     {
                //
                //         Console.WriteLine();
                //         Console.WriteLine(new string('=', 60));
                //         Console.WriteLine("[Action] Determining...");
                //
                //         stateMachine = stateMachine.Determine(_dverbose).RenameToNormalNames("s");
                //         if (_dverbose)
                //         {
                //             Console.WriteLine(new string('=', 60));
                //             Console.WriteLine("[Info] State machine was determined");
                //         }
                //         Console.WriteLine($"[Info] New id: {stateMachine.Id}");
                //         Console.WriteLine();
                //
                //         PrintTable(stateMachine);
                //         PrintDot(stateMachine);
                //
                //         SaveMachineIntoFile(stateMachine, "Determ");
                //     }
                //
                //     if (_minimize)
                //     {
                //         Console.WriteLine();
                //         Console.WriteLine(new string('=', 60));
                //         Console.WriteLine("[Action] Minimizing...");
                //
                //         stateMachine = stateMachine.Minimize().RenameToNormalNames("m");
                //         Console.WriteLine($"[Info] New id: {stateMachine.Id}");
                //         Console.WriteLine();
                //
                //         PrintTable(stateMachine);
                //         PrintDot(stateMachine);
                //         SaveMachineIntoFile(stateMachine, "MinDeterm");
                //     }
                //     
                //     TestAssets(Assets, stateMachine);
                //

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
                throw new NotImplementedException(nameof(SaveMachineIntoFile));
                // new Options {Arch = stateMachine.ToOptions(), Assets = Assets}
                //     .ToFile(Path.Combine(_outputPath, $"{fileName}Output_{stateMachine.Id.ToString().Substring(0, 7)}.json"));
            }
        }

        private static void PrintInfo(Machine stateMachine)
        {
            Console.WriteLine($"MachineID: {stateMachine.Id}  Initial state: {stateMachine.InitialState} States count: {stateMachine.GetStates().Count()} Transitions count: {stateMachine.GetTransitions().Count()}");
        }

        private static void PrintDot(Machine stateMachine)
        {
            // if (!_printDot) return;
            
            Console.WriteLine("Graph.dot");
            Console.WriteLine();
            Console.WriteLine(stateMachine.ToOptions().ToDot());
            Console.WriteLine();
        }

        private static void PrintTable(Machine stateMachine)
        {
            // if(!_printTable) return;
            
            Console.WriteLine("Table");
            Console.WriteLine();
            Console.WriteLine(stateMachine.ToOptions().ToTable());
            Console.WriteLine();
        }

        private static void TestAsset(Machine machine, string text, bool expected)
        {
            var result = machine.Run(text);
            
            Console.Write(new string(' ', 3));
            Console.Write($"Testing \"{text}\"… ".PadRight(17));
            Console.Write($"Expected: {expected}. ".PadRight(19));
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
