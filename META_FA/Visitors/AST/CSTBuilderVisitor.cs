using System.Collections.Generic;
using System.Linq;
using BFParser;
using BFParser.SyntaxTreeNodeVisitors;
using META_FA.CST;

namespace META_FA.Visitors.AST
{
    public class CSTBuilderVisitor : CoreSyntaxTreeNodeVisitor
    {
        private readonly List<RegexCST> _buffer = new List<RegexCST>();
        
        public override void Visit(SyntaxTreeNode syntaxTreeNode)
        {
            switch (syntaxTreeNode.RuleName)
            {
                case RegexpGrammar.Symbol:
                    _buffer.Add(new RegexSymbol(syntaxTreeNode.ParsedText));
                    break;
                
                case RegexpGrammar.Element:
                    Visit(syntaxTreeNode.Children[0].Children[1]);
                    break;
                
                case RegexpGrammar.Quantifier:
                    Visit(syntaxTreeNode.Children[0]);
                    
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
                            _buffer.Add(new RegexQuantifierOneOrInfinity(quantSubExpr));
                            break;
                    }
                    
                    break;
                
                case RegexpGrammar.Str:
                    foreach (var node in syntaxTreeNode.Children)
                    {
                        Visit(node);
                    }

                    if (syntaxTreeNode.Children.Count > 1)
                    {
                        var strSubExprs = _buffer.GetRange(_buffer.Count - syntaxTreeNode.Children.Count, syntaxTreeNode.Children.Count);
                        _buffer.RemoveRange(_buffer.Count - syntaxTreeNode.Children.Count, syntaxTreeNode.Children.Count);
                        
                        _buffer.Add(new RegexString(strSubExprs));
                    }
                    
                    break;
                
                case RegexpGrammar.Variant:
                    Visit(syntaxTreeNode.Children[0].Children[0]);
                    
                    var variantElement = _buffer[^1];
                    _buffer.Remove(variantElement);
                    
                    Visit(syntaxTreeNode.Children[1]);

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

        public override object GetResult()
        {
            return !_buffer.Any() ? null : _buffer[0];
        }
    }
}