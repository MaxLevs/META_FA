﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Regex_Parser;
using Regex_Parser.CST;
using Regex_Parser.Visitors.AST;
using StateMachineLib.Options;
using StateMachineLib.StateMachine;
using StateMachineLib.StateMachine.Exceptions;

namespace META_FA
{
    class Program
    {
        static string _optionsFilePath = "Examples/example_nfa_var.fa";
        private static string _regexpForParsing;
        private static string _outputPath;
        private static readonly List<Asset> Assets = new List<Asset>();
        private static bool _noMinimize;
        private static bool _noDeterm;
        
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
                            _regexpForParsing = args[0];
                            break;
                        case "-o":
                            var path = args[0];
                            if (Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute)
                                && Directory.Exists(path))
                            {
                                _outputPath = path;
                            }
                            break;
                        case "-M":
                            _noMinimize = true;
                            break;
                        
                        case "-D":
                            _noDeterm = true;
                            break;
                    }
                    args = args.Where((_, i) => i != 0).ToArray();
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
                    Assets.AddRange(options.Assets);
                }

                else
                {
                    Console.WriteLine($"[Info] Generating state machine with regex \"{_regexpForParsing}\"");
                    
                    var regexpParser = RegexpGrammar.GetParser();
                    var parseRes = regexpParser.Goal.Parse(_regexpForParsing);

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
                TestAssets(Assets, stateMachine);
                SaveMachineIntoFile(stateMachine, "NonDeterm");

                if (!_noDeterm && stateMachine.Type != MachineType.Determined)
                {
                    
                    Console.WriteLine();
                    Console.WriteLine(new string('=', 60));
                    Console.WriteLine("[Action] Determining...");

                    stateMachine = stateMachine.Determine().RenameToNormalNames("s");
                    Console.WriteLine($"  [Info] New id: {stateMachine.Id}");
                    Console.WriteLine();
                    
                    PrintTable(stateMachine);
                    PrintDot(stateMachine);
                    
                    SaveMachineIntoFile(stateMachine,"Determ");
                }

                if (!_noMinimize)
                {
                    Console.WriteLine();
                    Console.WriteLine(new string('=', 60));
                    Console.WriteLine("[Action] Minimizing...");

                    stateMachine = stateMachine.Minimize().RenameToNormalNames("m");
                    Console.WriteLine($"  [Info] New id: {stateMachine.Id}");
                    Console.WriteLine();
                    
                    PrintTable(stateMachine);
                    PrintDot(stateMachine);
                    TestAssets(Assets, stateMachine);
                    SaveMachineIntoFile(stateMachine,"MinDeterm");
                }
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

        private static void TestAssets(IEnumerable<Asset> assets, Machine stateMachine)
        {
            var enumerable = assets.ToList();
            if (enumerable.Any())
            {
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("[Action] Testing assets...");
                Console.WriteLine();

                // foreach (var (text, expectedRes) in options.Assets)
                foreach (var asset in enumerable)
                {
                    Console.Write(new string(' ', 3));
                    Console.Write($"Testing \"{asset.Text}\"… ".PadRight(enumerable.Max(x => x.Text?.Length ?? 4) + 13));
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
