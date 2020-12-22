using System;
using System.Collections.Generic;
using System.Linq;
using BFParser;
using BFParser.SyntaxTreeNodeVisitors;
using DSL_Parser.CST;

namespace DSL_Parser.Visitors.AST
{
    public class CstBuilderVisitor : CoreSyntaxTreeNodeVisitor
    {
        private readonly Stack<CstCoreNode> _nodes;
        
        public override dynamic GetResult()
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

                case DSLGrammar.String:
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
                    var codeEntities = new List<CstCodeEntity>();
                    
                    var astDeclareAreas = syntaxTreeNode.Children[0].Children;
                    
                    // Take declarations
                    foreach (var astDeclareArea in astDeclareAreas)
                    {
                        astDeclareArea.Visit(this); // DeclareArea

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

                    // Take code entities
                    if (syntaxTreeNode.Children[1].ParsedText != null)
                    {
                        var astCodeEntities = syntaxTreeNode.Children[1].Children[0].Children;
                        
                        foreach (var astCodeEntity in astCodeEntities)
                        {
                            astCodeEntity.Visit(this); // AssetRule
                            
                            var entity = (CstCodeEntity) _nodes.Pop();
                            
                            codeEntities.Add(entity);
                        }
                    }
                    
                    var dsl = new CstDsl(declarations, codeEntities);
                    
                    _nodes.Push(dsl);
                    
                    break;
                }

                case DSLGrammar.CodeArea:
                {
                    var result = new List<CstCodeEntity>();
                    foreach (var childNode in syntaxTreeNode.Children)
                    {
                        childNode.Visit(this);
                        var codeEntity = (CstCodeEntity) _nodes.Pop();
                        result.Add(codeEntity);
                    }
                    
                    var codeArea = new CstCodeArea(result);
                    _nodes.Push(codeArea);
                    
                    break;
                }

                case DSLGrammar.FunctionCall:
                {
                    syntaxTreeNode.Children[0].Visit(this);
                    syntaxTreeNode.Children[2].Children[0].Visit(this);

                    var someArg = _nodes.Pop();
                    CstFuncArgsList functionArgs;
                    if (someArg is CstIdentity || someArg is CstConstantValue)
                        functionArgs = new CstFuncArgsList(new List<CstFuncArg> {new CstFuncArg(someArg)});
                    else
                        functionArgs = (CstFuncArgsList) someArg;

                    var functionName = (CstIdentity) _nodes.Pop();
                    
                    var functionCall = new CstFunctionCall(functionName, functionArgs.Args);
                    _nodes.Push(functionCall);
                    
                    break;
                }

                case DSLGrammar.FuncArgsList:
                {
                    switch (syntaxTreeNode.Children.Count)
                    {
                        case 1:
                        {
                            syntaxTreeNode.Children[0].Visit(this);
                            var result = new List<CstFuncArg> {new CstFuncArg(_nodes.Pop())};
                            _nodes.Push(new CstFuncArgsList(result));
                            break;
                        }
                        
                        case 3:
                        {
                            syntaxTreeNode.Children[0].Visit(this);
                            var result = new List<CstFuncArg> {new CstFuncArg(_nodes.Pop())};
                            syntaxTreeNode.Children[2].Visit(this);
                            var node = _nodes.Pop();
                            if (node is CstFuncArgsList argsList)
                                result.AddRange(argsList.Args);
                            else
                                result.Add(new CstFuncArg(node));

                            _nodes.Push(new CstFuncArgsList(result));
                            break;
                        }
                        
                        default:
                            throw new NotImplementedException();
                            break;
                    }

                    break;
                }

                case DSLGrammar.Int:
                {
                    var numberText = syntaxTreeNode.ParsedText;
                    var number = int.Parse(numberText);
                    _nodes.Push(new CstInt(number));
                    break;
                }

                case DSLGrammar.Double:
                {
                    var numberText = syntaxTreeNode.ParsedText;
                    var number = double.Parse(numberText);
                    _nodes.Push(new CstDouble(number));
                    break;
                }

                case DSLGrammar.FunctionDefinition:
                {
                    syntaxTreeNode.Children[1].Visit(this);

                    var identity = (CstIdentity) _nodes.Pop();
                    
                    syntaxTreeNode.Children[3].Children[0].Visit(this);
                    
                    var args = new List<CstFuncDefArg>();
                    var someDefArg = _nodes.Pop();
                    switch (someDefArg)
                    {
                        case CstFuncDefArg funcDefArg:
                            args.Add(funcDefArg);
                            break;
                        case CstFuncDefArgList funcDefArgList:
                            args.AddRange(funcDefArgList.Args);
                            break;
                    }

                    var statements = new List<CstCoreNode>();
                    if (syntaxTreeNode.Children[5].Children.Any())
                    {
                        syntaxTreeNode.Children[5].Children[0].Visit(this);
                        throw new NotImplementedException();
                    }
                    
                    var functionDefinition = new CstFunctionDefinition(identity, args, statements);
                    _nodes.Push(functionDefinition);
                        
                    break;
                }
                
                case DSLGrammar.FuncDefArgsList:
                {
                    var argList = new List<CstFuncDefArg>();
                    
                    syntaxTreeNode.Children[0].Visit(this);
                    syntaxTreeNode.Children[2].Visit(this);

                    argList.Add((CstFuncDefArg) _nodes.Pop());
                    
                    var someArg = _nodes.Pop();
                    switch (someArg)
                    {
                        case CstFuncDefArg funcDefArg:
                            argList.Add(funcDefArg);
                            break;
                        
                        case CstFuncDefArgList defArgList:
                            argList.AddRange(defArgList.Args);
                            break;
                    }
                    
                    var funDefArgsList = new CstFuncDefArgList(argList);
                    _nodes.Push(funDefArgsList);
                    
                    break;
                }

                case DSLGrammar.FuncDefArg:
                {
                    var type = "identity";
                    if (syntaxTreeNode.Children[0].Children.Any())
                    {
                        type = syntaxTreeNode.Children[0].Children[0].ParsedText;
                    }

                    var data = syntaxTreeNode.Children[1].ParsedText;
                    
                    var funcDefArg = new CstFuncDefArg(type, data);
                    _nodes.Push(funcDefArg);
                    
                    break;
                }


                case DSLGrammar.DeclareArea:
                {
                    syntaxTreeNode.Children[1].Visit(this); // Identity
                    syntaxTreeNode.Children[2].Visit(this); // DeclareBody

                    break;
                }

                case DSLGrammar.DeclareBody:
                {
                    var isStateBlockThere = syntaxTreeNode.Children[0].ParsedText != null;

                    if (isStateBlockThere)
                    {
                        syntaxTreeNode.Children[0].Children[0].Visit(this); // StatesBlock
                        syntaxTreeNode.Children[1].Visit(this); // InitialBlock
                        syntaxTreeNode.Children[2].Visit(this); // FinalsBlock
                        syntaxTreeNode.Children[3].Visit(this); // TableBlock
                    }

                    else
                    {
                        syntaxTreeNode.Children[1].Visit(this); // InitialBlock
                        syntaxTreeNode.Children[2].Visit(this); // FinalsBlock
                        syntaxTreeNode.Children[3].Visit(this); // TableBlock
                    }
                    
                    break;
                }
                
                case DSLGrammar.FinalsBlock:
                {
                    syntaxTreeNode.Children[3].Visit(this); // StateList or StateName

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
                    syntaxTreeNode.Children[2].Visit(this); // StateName
                    
                    break;
                }
                
                case DSLGrammar.StatesBlock:
                {
                    syntaxTreeNode.Children[3].Visit(this); // StateList
                    
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
                    syntaxTreeNode.Children[0].Visit(this); // StateName
                    syntaxTreeNode.Children[2].Visit(this); // StateList or StateName

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
                        astRow.Visit(this); // TableRow
                    }
                    
                    break;
                }
                
                case DSLGrammar.TableRow:
                {
                    bool isEpsilon = syntaxTreeNode.Children[1].Children.Count == 2;

                    if (isEpsilon)
                    {
                        syntaxTreeNode.Children[1].Children[0].Visit(this); // NodeName: StartState
                        syntaxTreeNode.Children[1].Children[1].Visit(this); // NodeName: EndState

                        var endState = (CstStateName) _nodes.Pop();
                        var startState = (CstStateName) _nodes.Pop();

                        var transition = new CstTransition(startState, endState);
                        
                        _nodes.Push(transition);
                    }

                    else
                    {
                        syntaxTreeNode.Children[1].Children[0].Visit(this); //NodeName: StartState
                        syntaxTreeNode.Children[1].Children[1].Visit(this); //Str: Token
                        syntaxTreeNode.Children[1].Children[2].Visit(this); //NodeName: EndState
                        
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