using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GiGraph.Dot.Entities.Attributes.Enums;
using GiGraph.Dot.Entities.Graphs;
using GiGraph.Dot.Entities.Types.Styles;
using GiGraph.Dot.Extensions;

namespace StateMachineLib.Options
{
    public class SMOptions
    {
        public string MachineId { get; set; }
        public List<string> States { get; set; }
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


        public string ToTable()
        {
            var tokens = Transitions.Select(tr => tr.Token).Distinct().ToList();
            
            var states = GetStates().ToList();

            var table = Transitions.GroupBy(tr => new {tr.StartState, tr.Token}).ToList();

            var pad = table
                .GroupBy(tableCell => tableCell.Key.Token)
                .Select(x => Math.Max(x.Key?.Length ?? 3, x.Max(cell => Math.Max(cell.Key.StartState.Length, string.Join(",", cell.Select(tr => tr.EndState)).Length))) + 2)
                .ToList();
            var paddings = new List<int> { "  State".Length };
            paddings.AddRange(pad);
            var padding = paddings.Max();
            
            string result = "State".PadLeft(paddings[0]) + string.Join("", tokens.Select((token, i) => $"  {token ?? "[ε]"}".PadLeft(paddings[i+1]))) + "\n";
            foreach (var state in states)
            {
                result += state.PadLeft(paddings[0]);

                var i = 1;
                foreach (var token in tokens)
                {
                    var cell = table.Find(x => x.Key.StartState == state && x.Key.Token == token);

                    if (cell == null)
                    {
                        result += "  .".PadLeft(paddings[i++]); // new string(' ', paddings[i++]);
                        continue;
                    }

                    result += $" {string.Join(",", cell.Select(tr => tr.EndState))}".PadLeft(paddings[i++]);
                }

                result += "\n";
            }

            return result;

            // var states = GetStates().ToList();
            // var padding = Math.Max(states.Max(s => s.Length), Transitions.Max(tr => tr.Token?.Length ?? 1)) + 2;
            // if (Transitions.Any(tr => tr.IsEpsilon))
            // {
            //     padding = Math.Max(padding, 3 + 2);
            // }
            // var headline = states.Aggregate(">".PadLeft(padding), (result, stateCol) => result + stateCol.PadLeft(padding));
            // return states.Aggregate(headline, (resultRow, stateRow) => resultRow + "\n" + states.Aggregate(stateRow.PadLeft(padding),
            //     (resultCol, stateCol) =>
            //     {
            //         var tr = Transitions.Find(tr => tr.StartState == stateCol && tr.EndState == stateRow);
            //         
            //         if (tr == null)
            //         {
            //             return resultCol + ".".PadLeft(padding);
            //         }
            //         
            //         return resultCol + (tr.Token ?? (tr.IsEpsilon ? "[ε]" : ".")).PadLeft(padding);
            //     }));
        }

        public string ToDot()
        {
            var graph = new DotGraph(directed: true);
            graph.Attributes.Label = "StateMachine";
            graph.Attributes.LayoutDirection = DotLayoutDirection.LeftToRight;
            graph.Nodes.Attributes.Style.FillStyle = DotNodeFillStyle.Normal;
            graph.Nodes.Attributes.FillColor = Color.White;

            graph.Nodes.Add(MachineId, attributes =>
            {
                attributes.Label = "";
                attributes.Style.Invisible = true;
            });
            graph.Edges.Add(MachineId, InitialState);

            graph.Nodes.Add(InitialState, state =>
            {
                state.FillColor = Color.Gray;
            });
            
            graph.Nodes.AddRange(FinalStates, node =>
            {
                node.Attributes.Shape = DotNodeShape.DoubleCircle;
                node.Attributes.FillColor = Color.Gray;
            });

            foreach (var group in Transitions.GroupBy(options => new {options.StartState, options.EndState}))
            {
                graph.Edges.Add(group.Key.StartState, group.Key.EndState, edge =>
                {
                    edge.Attributes.Label = string.Join(",", group.Select(options => options.Token ?? "[ε]"));
                });
            }
            
            return graph.Build();
        }
    }
}