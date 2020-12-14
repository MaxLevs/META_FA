using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace META_FA.Options
{
    public class SMOptions
    {
        public string InitialState { get; set; }
        public List<string> FinalStates { get; set; }
        public List<TransitionOptions> Transitions { get; set; }

        public IEnumerable<string> GetStates()
        {
            var firstPart = Transitions.Select(options => options.StartState).ToList();
            var secondPart = Transitions.Select(options => options.EndState).ToList();
            firstPart.AddRange(secondPart);
            return firstPart.Distinct().ToList();
        }

        public static SMOptions FromFile(string path)
        {
            using var optionsFile = File.OpenText(path);
            return JsonSerializer.Deserialize<SMOptions>(optionsFile.ReadToEnd());
        }

        public string ToTable()
        {
            var states = GetStates().ToList();
            var padding = Math.Max(states.Max(s => s.Length), Transitions.Max(tr => tr.Token.Length)) + 2;
            var headline = states.Aggregate(new string(' ', padding), (result, stateCol) => result + stateCol.PadLeft(padding));
            return states.Aggregate(headline, (resultRow, stateRow) => resultRow + "\n" + states.Aggregate(stateRow.PadLeft(padding), (resultCol, stateCol) => resultCol + (Transitions.Find(tr => tr.StartState == stateCol && tr.EndState == stateRow)?.Token ?? ".").PadLeft(padding)));
        }
    }
}