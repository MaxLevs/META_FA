using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DSL_Parser.CST;
using GiGraph.Dot.Entities.Attributes.Enums;
using GiGraph.Dot.Entities.Graphs;
using GiGraph.Dot.Entities.Nodes;
using GiGraph.Dot.Entities.Types.Styles;
using GiGraph.Dot.Extensions;

namespace DSL_Parser.Visitors.CST
{
    public class CstDotVisitor : CstCoreVisitor
    {
        private readonly DotGraph _graph;

        public CstDotVisitor()
        {
            _graph = new DotGraph(true);
            _graph.Attributes.Style.FillStyle = DotClusterFillStyle.Normal;
            _graph.Attributes.Label = "State machine DSL CST";
            _graph.Attributes.EdgeShape = DotEdgeShape.Orthogonal;
            // _graph.Nodes.Attributes.Style.FillStyle = DotNodeFillStyle.Normal;
            _graph.Nodes.Attributes.Shape = DotNodeShape.Box;
            // _graph.Nodes.Attributes.FillColor = Color.DarkGray;
        }

        public override object GetResult()
        {
            return _graph.Build();
        }

        public override void Apply(CstIdentity cstIdentity)
        {
            var node = new DotNode(cstIdentity.IdShort);
            node.Attributes.Label = $"Identity[{cstIdentity.Name}]";
            _graph.Nodes.Add(node);
        }

        public override void Apply(CstSymbol cstSymbol)
        {
            var node = new DotNode(cstSymbol.IdShort);
            node.Attributes.Label = $"Symbol[{cstSymbol.Name}]";
            _graph.Nodes.Add(node);
        }

        public override void Apply(CstString cstString)
        {
            var node = new DotNode(cstString.IdShort);
            node.Attributes.Label = $"String[{cstString.Data}]";
            _graph.Nodes.Add(node);
        }

        public override void Apply(CstStateName cstStateName)
        {
            var node = new DotNode(cstStateName.IdShort);
            node.Attributes.Label = $"StateName[{cstStateName.Name}]";
            _graph.Nodes.Add(node);
        }

        public override void Apply(CstStatesList cstStatesList)
        {
            throw new System.NotImplementedException();
        }

        public override void Apply(CstTransition cstTransition)
        {
            var node = new DotNode(cstTransition.IdShort);
            
            if (cstTransition.IsEpsilon)
            {
                node.Attributes.Label = $"Transition[ε: {cstTransition.StartStateName.Name} ==> {cstTransition.EndStateName.Name}]";
                _graph.Nodes.Add(node);
                
                cstTransition.StartStateName.Visit(this);
                cstTransition.EndStateName.Visit(this);

                _graph.Edges.Add(node.Id, cstTransition.StartStateName.IdShort);
                _graph.Edges.Add(node.Id, cstTransition.EndStateName.IdShort);
            }

            else
            {
                node.Attributes.Label = $"Transition[{cstTransition.StartStateName.Name} ==[{cstTransition.Token.Name}]=> {cstTransition.EndStateName.Name}]";
                _graph.Nodes.Add(node);
                
                cstTransition.StartStateName.Visit(this);
                cstTransition.Token.Visit(this);
                cstTransition.EndStateName.Visit(this);

                _graph.Edges.Add(node.Id, cstTransition.StartStateName.IdShort);
                _graph.Edges.Add(node.Id, cstTransition.Token.IdShort);
                _graph.Edges.Add(node.Id, cstTransition.EndStateName.IdShort);
            }
        }

        public override void Apply(CstDeclaration cstDeclaration)
        {
            var node = new DotNode(cstDeclaration.IdShort);
            node.Attributes.Label = $"Declaration.{cstDeclaration.Identity.Name}";
            _graph.Nodes.Add(node);
            
            cstDeclaration.Identity.Visit(this);
            _graph.Edges.Add(node.Id, cstDeclaration.Identity.IdShort);

            if (cstDeclaration.States != null)
            {
                var statesBlockNode = CreateAttributeNode(nameof(cstDeclaration.States));
                _graph.Nodes.Add(statesBlockNode);
                
                foreach (var state in cstDeclaration.States)
                {
                    state.Visit(this);
                    _graph.Edges.Add(statesBlockNode.Id, state.IdShort);
                }

                _graph.Edges.Add(node.Id, statesBlockNode.Id);
            }

            var initialBlockNode = CreateAttributeNode(nameof(cstDeclaration.InitialStateName));
            _graph.Nodes.Add(initialBlockNode);
            
            cstDeclaration.InitialStateName.Visit(this);

            _graph.Edges.Add(initialBlockNode.Id, cstDeclaration.InitialStateName.IdShort);
            _graph.Edges.Add(node.Id, initialBlockNode.Id);

            var finalBlockNode = CreateAttributeNode(nameof(cstDeclaration.Finals));
            _graph.Nodes.Add(finalBlockNode);
            
            foreach (var final in cstDeclaration.Finals)
            {
                final.Visit(this);
                _graph.Edges.Add(finalBlockNode.Id, final.IdShort);
            }

            _graph.Edges.Add(node.Id, finalBlockNode.Id);

            var tableBlockNode = CreateAttributeNode(nameof(cstDeclaration.Trancitions));
            _graph.Nodes.Add(tableBlockNode);

            foreach (var transition in cstDeclaration.Trancitions)
            {
                transition.Visit(this);
                _graph.Edges.Add(tableBlockNode.Id, transition.IdShort);
            }

            _graph.Edges.Add(node.Id, tableBlockNode.Id);
        }

        public override void Apply(CstAsset cstAsset)
        {
            var node = new DotNode(cstAsset.IdShort);
            node.Attributes.Label = $"Asset.{cstAsset.Identity.Name}[\"{cstAsset.String}\"]";
            _graph.Nodes.Add(node);
            
            cstAsset.Identity.Visit(this);

            _graph.Edges.Add(node.Id, cstAsset.Identity.IdShort);
        }

        public override void Apply(CstDsl cstDsl)
        {
            var node = new DotNode(cstDsl.IdShort);
            node.Attributes.Label = "DSL";
            _graph.Nodes.Add(node);

            var declarationAreaNode = CreateAttributeNode(nameof(cstDsl.Declarations));
            _graph.Nodes.Add(declarationAreaNode);
            
            foreach (var declaration in cstDsl.Declarations)
            {
                declaration.Visit(this);
                _graph.Edges.Add(declarationAreaNode.Id, declaration.IdShort);
            }
            
            _graph.Edges.Add(node.Id, declarationAreaNode.Id);

            if (!cstDsl.Assets.Any()) return;

            var assetsAreaNode = CreateAttributeNode(nameof(cstDsl.Assets));
            _graph.Nodes.Add(assetsAreaNode);
            
            foreach (var asset in cstDsl.Assets)
            {
                asset.Visit(this);
                _graph.Edges.Add(assetsAreaNode.Id, asset.IdShort);
            }
            
            _graph.Edges.Add(node.Id, assetsAreaNode.Id);
        }

        public static DotNode CreateAttributeNode(string label)
        {
            var node =new DotNode(Guid.NewGuid().ToString().Substring(0,7));
            node.Attributes.Label = label;
            node.Attributes.Shape = DotNodeShape.Ellipse;
            return node;
        }
    }
}