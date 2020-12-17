using GiGraph.Dot.Entities.Graphs;
using GiGraph.Dot.Entities.Nodes;
using META_FA.CST;

namespace META_FA.Visitors.CST
{
    public class CSTDotVisitor : CSTVisitor
    {
        private DotGraph _graph = new DotGraph();
        
        public override void Apply(RegexCST cstNode)
        {
            switch (cstNode)
            {
                case RegexSymbol regexSymbol:
                {
                    var node = new DotNode(regexSymbol.Id.ToString());
                    node.Attributes.Label = regexSymbol.Token;
                    _graph.Nodes.Add(node);
                    break;
                }

                case RegexQuantifierMaybe regexQuantifierMaybe:
                {
                    break;
                }

                case RegexQuantifierOneOrInfinity regexQuantifierOneOrInfinity:
                {
                    break;
                }

                case RegexQuantifierZeroOrInfinity regexQuantifierZeroOrInfinity:
                {
                    break;
                }

                case RegexString regexString:
                {
                    break;
                }

                case RegexVariant regexVariant:
                {
                    break;
                }
            }
        }
    }
}