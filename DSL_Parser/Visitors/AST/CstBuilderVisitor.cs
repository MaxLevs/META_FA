using System;
using System.Collections.Generic;
using BFParser;
using DSL_Parser.CST;

namespace DSL_Parser.Visitors.AST
{
    public class CstBuilderVisitor : AstCoreVisitor
    {
        private readonly Stack<CstCoreNode> _nodes;
        
        public override object GetResult()
        {
            _nodes.TryPeek(out var result);
            return result;
        }

        public CstBuilderVisitor()
        {
            _nodes = new Stack<CstCoreNode>();
        }

        public override void Apply(SyntaxTreeNode syntaxTreeNode)
        {
            switch (syntaxTreeNode.RuleName)
            {
                case DSLGrammar.Identity:
                {
                    var name = syntaxTreeNode.ParsedText;
                    var identityNode = new CstIdentity(name);
                    
                    _nodes.Push(identityNode);
                    
                    break;
                }

                case DSLGrammar.Symbol:
                {
                    var name = syntaxTreeNode.ParsedText;
                    var symbolNode = new CstSymbol(name);
                    
                    _nodes.Push(symbolNode);
                    
                    break;
                }

                case DSLGrammar.Str:
                {
                    var @string = syntaxTreeNode.ParsedText;
                    var stringNode = new CstString(@string[1..^1]); // Delete "
                    
                    _nodes.Push(stringNode);
                    
                    break;
                }

                case DSLGrammar.Bool:
                {
                    var @bool = syntaxTreeNode.ParsedText switch
                    {
                        "true" => true,
                        "false" => false,
                        _ => throw new ArgumentException("DSLGrammar.Bool result must be either true or false")
                    };
                    
                    var boolNode = new CstBool(@bool);
                    
                    _nodes.Push(boolNode);
                        
                    break;
                }
                
                case DSLGrammar.StateName:
                {
                    var name = syntaxTreeNode.ParsedText;
                    var stateNameNode = new CstStateName(name);
                    
                    _nodes.Push(stateNameNode);
                    
                    break;
                }

                case DSLGrammar.Dsl:
                {
                    var declarations = new List<CstDeclaration>();
                    var assets = new List<CstAsset>();
                    
                    var astDeclareAreas = syntaxTreeNode.Children[0].Children;
                    
                    // Take declarations
                    foreach (var astDeclareArea in astDeclareAreas)
                    {
                        Apply(astDeclareArea); // DeclareArea

                        var isStatesBlockThere = astDeclareArea.Children[2].Children[0].ParsedText != null;

                        if (isStatesBlockThere)
                        {
                            var transitionsCount = astDeclareArea.Children[2].Children[3].Children[3].Children.Count;
                            
                            var transitions = new List<CstTransition>();
                            
                            for (var k = 0; k < transitionsCount; k++)
                            {
                                var transition = (CstTransition) _nodes.Pop();
                                
                                transitions.Add(transition);
                            }

                            var finals = (CstStatesList) _nodes.Pop();
                            var initial = (CstStateName) _nodes.Pop();
                            var states = (CstStatesList) _nodes.Pop();
                            var identity = (CstIdentity) _nodes.Pop();
                            
                            var declaration = new CstDeclaration(identity, states.StateNames, initial, finals.StateNames, transitions);
                            
                            declarations.Add(declaration);
                        }

                        else
                        {
                            var transitionsCount = astDeclareArea.Children[2].Children[3].Children[3].Children.Count;
                            
                            var transitions = new List<CstTransition>();
                            
                            for (var k = 0; k < transitionsCount; k++)
                            {
                                var transition = (CstTransition) _nodes.Pop();
                                
                                transitions.Add(transition);
                            }
                            
                            transitions.Reverse();

                            var finals = (CstStatesList) _nodes.Pop();
                            var initial = (CstStateName) _nodes.Pop();
                            var identity = (CstIdentity) _nodes.Pop();
                            
                            var declaration = new CstDeclaration(identity, null, initial, finals.StateNames, transitions);
                            
                            declarations.Add(declaration);
                        }
                    }

                    // Take assets
                    if (syntaxTreeNode.Children[1].ParsedText != null)
                    {
                        var astAssets = syntaxTreeNode.Children[1].Children[0].Children;
                        
                        foreach (var astAsset in astAssets)
                        {
                            Apply(astAsset); // AssetRule
                            
                            var asset = (CstAsset) _nodes.Pop();
                            
                            assets.Add(asset);
                        }
                    }
                    
                    var dsl = new CstDsl(declarations, assets);
                    
                    _nodes.Push(dsl);
                    
                    break;
                }

                case DSLGrammar.AssetArgs:
                {
                    Apply(syntaxTreeNode.Children[0]); // Identity
                    Apply(syntaxTreeNode.Children[2]); // Str
                    Apply(syntaxTreeNode.Children[4]); // Bool
                    
                    break;
                }

                case DSLGrammar.AssetRule:
                {
                    Apply(syntaxTreeNode.Children[2]); // AssetArgs

                    var boolNode = (CstBool) _nodes.Pop();
                    var stringNode = (CstString) _nodes.Pop();
                    var identityNode = (CstIdentity) _nodes.Pop();
                    
                    var assetNode = new CstAsset(identityNode, stringNode.Data, boolNode.Data);
                    
                    _nodes.Push(assetNode);
                    
                    break;
                }

                case DSLGrammar.AssetsArea:
                {
                    var astAssets = syntaxTreeNode.Children;
                    
                    foreach (var asset in astAssets)
                    {
                        Apply(asset); // Asset
                    }
                    
                    break;
                }

                case DSLGrammar.DeclareArea:
                {
                    Apply(syntaxTreeNode.Children[1]); // Identity
                    Apply(syntaxTreeNode.Children[2]); // DeclareBody

                    break;
                }

                case DSLGrammar.DeclareBody:
                {
                    var isStateBlockThere = syntaxTreeNode.Children[0].ParsedText != null;

                    if (isStateBlockThere)
                    {
                        Apply(syntaxTreeNode.Children[0].Children[0]); // StatesBlock
                        Apply(syntaxTreeNode.Children[1]); // InitialBlock
                        Apply(syntaxTreeNode.Children[2]); // FinalsBlock
                        Apply(syntaxTreeNode.Children[3]); // TableBlock
                    }

                    else
                    {
                        Apply(syntaxTreeNode.Children[1]); // InitialBlock
                        Apply(syntaxTreeNode.Children[2]); // FinalsBlock
                        Apply(syntaxTreeNode.Children[3]); // TableBlock
                    }
                    
                    break;
                }
                
                case DSLGrammar.FinalsBlock:
                {
                    Apply(syntaxTreeNode.Children[3]); // StateList or StateName

                    _nodes.TryPop(out var resNode);
                    
                    switch (resNode)
                    {
                        case CstStateName stateNameNode:
                            var statesList = new CstStatesList(new List<CstStateName>{stateNameNode});
                            _nodes.Push(statesList);
                            break;
                            
                        case CstStatesList _:
                            _nodes.Push(resNode);
                            break;
                    }
                    
                    break;
                }
                
                case DSLGrammar.InitialBlock:
                {
                    Apply(syntaxTreeNode.Children[2]); // StateName
                    
                    break;
                }
                
                case DSLGrammar.StatesBlock:
                {
                    Apply(syntaxTreeNode.Children[3]); // StateList
                    
                    _nodes.TryPop(out var resNode);
                    
                    switch (resNode)
                    {
                        case CstStateName stateNameNode:
                            var statesList = new CstStatesList(new List<CstStateName>{stateNameNode});
                            _nodes.Push(statesList);
                            break;
                            
                        case CstStatesList _:
                            _nodes.Push(resNode);
                            break;
                    }
                    
                    break;
                }
                
                case DSLGrammar.StatesList:
                {
                    Apply(syntaxTreeNode.Children[0]); // StateName
                    Apply(syntaxTreeNode.Children[2]); // StateList or StateName

                    var lastListElement = _nodes.Pop();

                    if (lastListElement is CstStatesList statesListPrev)
                    {
                        var stateName = (CstStateName) _nodes.Pop();
                        var stateNames = new List<CstStateName>(statesListPrev.StateNames);
                        
                        stateNames.Insert(0, stateName);
                        
                        var statesList = new CstStatesList(stateNames);

                        _nodes.Push(statesList);
                    }

                    else
                    {
                        var stateName = (CstStateName) _nodes.Pop();
                        var stateName2 = (CstStateName) lastListElement;
                        
                        var statesList = new CstStatesList(new List<CstStateName> {stateName, stateName2});
                        
                        _nodes.Push(statesList);
                    }
                    
                    break;
                }
                
                case DSLGrammar.TableBlock:
                {
                    var astRows = syntaxTreeNode.Children[3].Children;
                    
                    foreach (var astRow in astRows)
                    {
                        Apply(astRow); // TableRow
                    }
                    
                    break;
                }
                
                case DSLGrammar.TableRow:
                {
                    bool isEpsilon = syntaxTreeNode.Children[1].Children.Count == 2;

                    if (isEpsilon)
                    {
                        Apply(syntaxTreeNode.Children[1].Children[0]); // NodeName: StartState
                        Apply(syntaxTreeNode.Children[1].Children[1]); // NodeName: EndState

                        var endState = (CstStateName) _nodes.Pop();
                        var startState = (CstStateName) _nodes.Pop();

                        var transition = new CstTransition(startState, endState);
                        
                        _nodes.Push(transition);
                    }

                    else
                    {
                        Apply(syntaxTreeNode.Children[1].Children[0]); //NodeName: StartState
                        Apply(syntaxTreeNode.Children[1].Children[1]); //Str: Token
                        Apply(syntaxTreeNode.Children[1].Children[2]); //NodeName: EndState
                        
                        var endState = (CstStateName) _nodes.Pop();
                        var token = (CstSymbol) _nodes.Pop();
                        var startState = (CstStateName) _nodes.Pop();

                        var transition = new CstTransition(startState, token, endState);
                        
                        _nodes.Push(transition);
                    }
                    
                    break;
                }
            }
        }
    }
}