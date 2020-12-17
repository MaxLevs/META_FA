using System.Collections.Generic;
using GiGraph.Dot.Entities.Edges;
using GiGraph.Dot.Entities.Graphs;
using GiGraph.Dot.Entities.Nodes;
using GiGraph.Dot.Extensions;
using META_FA.CST;

namespace META_FA.Visitors.CST
{
    public class CSTDotVisitor : CSTVisitor
    {
        private readonly DotGraph _graph = new DotGraph(directed: true);
        private readonly List<DotNode> _nodes = new List<DotNode>();
        
        public string Result {
            get
            {
                _graph.Attributes.Label = "Concrete Syntax Tree";
                _graph.Attributes.Center = true;
                return _graph.Build();
            }
        }
        
        public override void Apply(RegexCST cstNode)
        {
            switch (cstNode)
            {
                case RegexSymbol regexSymbol:
                {
                    var node = new DotNode(regexSymbol.Id.ToString());
                    node.Attributes.Label = regexSymbol.Token;
                    _graph.Nodes.Add(node);
                    _nodes.Add(node);
                    break;
                }

                case RegexQuantifierMaybe regexQuantifierMaybe:
                {
                    regexQuantifierMaybe.Child.Visit(this);
                    
                    var node = new DotNode(regexQuantifierMaybe.Id.ToString());
                    node.Attributes.Label = "[Maybe]";
                    _graph.Nodes.Add(node);

                    var edge = new DotEdge(node.Id, _nodes[^1].Id);
                    _graph.Edges.Add(edge);
                    
                    _nodes.Add(node);
                    
                    break;
                }

                case RegexQuantifierOneOrInfinity regexQuantifierOneOrInfinity:
                {
                    regexQuantifierOneOrInfinity.Child.Visit(this);
                    
                    var node = new DotNode(regexQuantifierOneOrInfinity.Id.ToString());
                    node.Attributes.Label = "[One or Infinity]";
                    _graph.Nodes.Add(node);

                    var edge = new DotEdge(node.Id, _nodes[^1].Id);
                    _graph.Edges.Add(edge);
                    
                    _nodes.Add(node);
                    
                    break;
                }

                case RegexQuantifierZeroOrInfinity regexQuantifierZeroOrInfinity:
                {
                    regexQuantifierZeroOrInfinity.Child.Visit(this);
                    
                    var node = new DotNode(regexQuantifierZeroOrInfinity.Id.ToString());
                    node.Attributes.Label = "[Zero Or Infinity]";
                    _graph.Nodes.Add(node);

                    var edge = new DotEdge(node.Id, _nodes[^1].Id);
                    _graph.Edges.Add(edge);
                    
                    _nodes.Add(node);
                    
                    break;
                }

                case RegexString regexString:
                {
                    var nodes = new List<DotNode>();
                    foreach (var child in regexString.Children)
                    {
                        child.Visit(this);
                        nodes.Add(_nodes[^1]);
                    }
                    
                    var currentNode = new DotNode(regexString.Id.ToString());
                    currentNode.Attributes.Label = "{String}";
                    _graph.Nodes.Add(currentNode);

                    foreach (var node in nodes)
                    {
                        _graph.Edges.Add(new DotEdge(currentNode.Id, node.Id));
                    }
                    
                    _nodes.Add(currentNode);
                    
                    break;
                }

                case RegexVariant regexVariant:
                {
                    var nodes = new List<DotNode>();
                    foreach (var child in regexVariant.Children)
                    {
                        child.Visit(this);
                        nodes.Add(_nodes[^1]);
                    }
                    
                    var currentNode = new DotNode(regexVariant.Id.ToString());
                    currentNode.Attributes.Label = "{Variant}";
                    _graph.Nodes.Add(currentNode);

                    foreach (var node in nodes)
                    {
                        _graph.Edges.Add(new DotEdge(currentNode.Id, node.Id));
                    }
                    
                    _nodes.Add(currentNode);
                    
                    break;
                }
            }
        }
    }
}