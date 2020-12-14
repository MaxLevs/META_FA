using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using META_FA.StateMachine;

namespace META_FA
{
    class Program
    {
        static void Main(string[] args)
        {
            var stateMachine = GetSM();
            var assets = new Dictionary<string, bool>
            {
                // ba((ab)|(ac))*cb
                {"bacb", true},
                {"bcb", false},
                {"baabcb", true},
                {"baaccb", true},
                {"baacababcb", true},
            };

            foreach (var (text, expectedRes) in assets)
            {
                Console.WriteLine($"Test \"{text}\". Expected: {expectedRes}. Result: {(stateMachine.Run(text) == expectedRes ? "Correct" : "Reject!")}");
            }
        }

        public static StateMachine.StateMachine GetSM()
        {
            using var jsonFile = File.OpenText("../../../regexMachine.json");
            var jsonText = jsonFile.ReadToEnd();
            var options = JsonSerializer.Deserialize<SMOptions>(jsonText);

            var stateMachine = new StateMachine.StateMachine();
            
            var statesNames = options.GetStates();
            var statesDict = new Dictionary<string, State>();
            foreach (var stateName in statesNames)
            {
                var state = new State(stateName, options.FinalStates.Contains(stateName));
                statesDict.Add(stateName, state);
                stateMachine.AddState(state);
            }

            foreach (var transitionOptions in options.Transitions)
            {
                stateMachine.AddTransition(new Transition(
                    statesDict[transitionOptions.StartState],
                    transitionOptions.Token,
                    statesDict[transitionOptions.EndState]
                ));
            }
            
            stateMachine.Init(options.InitialState);
            
            return stateMachine;
        }
    }
}