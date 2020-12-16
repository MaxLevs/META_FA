using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using GiGraph.Dot.Entities.Attributes.Enums;
using GiGraph.Dot.Entities.Graphs;
using GiGraph.Dot.Entities.Types.Styles;
using GiGraph.Dot.Extensions;

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


        public string ToTable()
        {
            var states = GetStates().ToList();
            var padding = Math.Max(states.Max(s => s.Length), Transitions.Max(tr => tr.Token?.Length ?? 1)) + 2;
            if (Transitions.Any(tr => tr.IsEpsilon))
            {
                padding = Math.Max(padding, 3 + 2);
            }
            var headline = states.Aggregate(">".PadLeft(padding), (result, stateCol) => result + stateCol.PadLeft(padding));
            return states.Aggregate(headline, (resultRow, stateRow) => resultRow + "\n" + states.Aggregate(stateRow.PadLeft(padding),
                (resultCol, stateCol) =>
                {
                    var tr = Transitions.Find(tr => tr.StartState == stateCol && tr.EndState == stateRow);
                    
                    if (tr == null)
                    {
                        return resultCol + ".".PadLeft(padding);
                    }
                    
                    return resultCol + (tr.Token ?? (tr.IsEpsilon ? "[ε]" : ".")).PadLeft(padding);
                }));
        }

        public string ToDot()
        {
            var graph = new DotGraph(directed: true);
            graph.Attributes.Label = "StateMachine";
            graph.Nodes.Attributes.Style.FillStyle = DotNodeFillStyle.Normal;
            graph.Nodes.Attributes.FillColor = Color.White;

            graph.Nodes.Add(InitialState, state => { state.FillColor = Color.Gray; });
            
            graph.Nodes.AddRange(FinalStates, node =>
            {
                node.Attributes.Shape = DotNodeShape.DoubleCircle;
                node.Attributes.FillColor = Color.Gray;
            });

            foreach (var @group in Transitions.GroupBy(options => new {options.StartState, options.EndState}))
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