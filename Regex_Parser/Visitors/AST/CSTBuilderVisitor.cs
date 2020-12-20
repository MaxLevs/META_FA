using System.Collections.Generic;
using System.Linq;
using BFParser;
using BFParser.SyntaxTreeNodeVisitors;
using Regex_Parser.CST;

namespace Regex_Parser.Visitors.AST
{
    public class CSTBuilderVisitor : CoreSyntaxTreeNodeVisitor
    {
        private readonly List<RegexCST> _buffer = new List<RegexCST>();
        
        public override void Apply(SyntaxTreeNode syntaxTreeNode)
        {
            switch (syntaxTreeNode.RuleName)
            {
                case RegexpGrammar.Symbol:
                    _buffer.Add(new RegexSymbol(syntaxTreeNode.ParsedText));
                    break;
                
                case RegexpGrammar.Element:
                    if (syntaxTreeNode.Children.Count == 1)
                    {
                        Apply(syntaxTreeNode.Children[0]);
                    }
                    else
                    {
                        Apply(syntaxTreeNode.Children[1]);
                    }
                    break;
                
                case RegexpGrammar.Quantifier:
                    Apply(syntaxTreeNode.Children[0]);
                    
                    var quantSubExpr = _buffer[^1];
                    _buffer.Remove(quantSubExpr);
                    
                    switch (syntaxTreeNode.Children[1].ParsedText)
                    {
                        case "?":
                            _buffer.Add(new RegexQuantifierMaybe(quantSubExpr));
                            break;
                        case "+":
                            _buffer.Add(new RegexQuantifierOneOrInfinity(quantSubExpr));
                            break;
                        case "*":
                            _buffer.Add(new RegexQuantifierZeroOrInfinity(quantSubExpr));
                            break;
                    }
                    
                    break;
                
                case RegexpGrammar.Str:
                    foreach (var node in syntaxTreeNode.Children)
                    {
                        Apply(node);
                    }

                    if (syntaxTreeNode.Children.Count > 1)
                    {
                        // var strSubExprs = _buffer.ToArray()[^syntaxTreeNode.Children.Count..]
                        var rangeStart = _buffer.Count - syntaxTreeNode.Children.Count;
                        var rangeCount = syntaxTreeNode.Children.Count;

                        var strSubExpressions = _buffer.GetRange(rangeStart, rangeCount);
                        _buffer.RemoveRange(rangeStart, rangeCount);
                        
                        _buffer.Add(new RegexString(strSubExpressions));
                    }
                    
                    break;
                
                case RegexpGrammar.Variant:
                    Apply(syntaxTreeNode.Children[0]);
                    
                    var variantElement = _buffer[^1];
                    _buffer.Remove(variantElement);
                    
                    Apply(syntaxTreeNode.Children[2]);

                    var suspectedElement = _buffer[^1];
                    _buffer.Remove(suspectedElement);

                    if (suspectedElement is RegexVariant anotherVariants)
                    {
                        var element = new List<RegexCST>(anotherVariants.Children);
                        element.Insert(0, variantElement);
                        
                        _buffer.Add(new RegexVariant(element));
                    }

                    else
                    {
                        _buffer.Add(new RegexVariant(new List<RegexCST> {variantElement, suspectedElement}));
                    }
                    
                    break;
            }
        }

        public override dynamic GetResult()
        {
            return !_buffer.Any() ? null : _buffer[0];
        }
    }
}