using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using META_FA.Assets;
using META_FA.Options;
using META_FA.StateMachine;

namespace META_FA
{
    class Program
    {
        static void Main(string[] args)
        {
            using var optionsFile = File.OpenText("machine_arch.json");
            var options = JsonSerializer.Deserialize<SMOptions>(optionsFile.ReadToEnd());
            
            using var assetsFile = File.OpenText("assets.json");
            var assets = JsonSerializer
                .Deserialize<List<Asset>>(assetsFile.ReadToEnd())
                .ToDictionary(asset => asset.Text, asset => asset.ExpectedResult);
            
            var stateMachine = Machine.GetFromOptions(options);
            
            foreach (var (text, expectedRes) in assets)
            {
                Console.WriteLine($"Test \"{text}\". Expected: {expectedRes}. Result: {(stateMachine.Run(text) == expectedRes ? "Correct" : "Reject!")}");
            }
        }
    }
}
