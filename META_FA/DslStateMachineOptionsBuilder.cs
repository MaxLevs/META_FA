using System.Collections.Generic;
using System.Linq;
using DSL_Parser.CST;
using DSL_Parser.Visitors.CST;
using StateMachineLib.Options;

namespace META_FA
{
    public class DslStateMachineOptionsBuilder : CstCoreVisitor
    {
        private readonly Dictionary<string, Options> _options;
        private readonly Stack<object> _buffer;

        public DslStateMachineOptionsBuilder()
        {
            _options = new Dictionary<string, Options>();
            _buffer = new Stack<object>();
        }

        public override object GetResult()
        {
            return _options;
        }

        public override void Apply(CstIdentity cstIdentity)
        {
            _buffer.Push(cstIdentity.Name);
        }

        public override void Apply(CstSymbol cstSymbol)
        {
            _buffer.Push(cstSymbol.Name);
        }

        public override void Apply(CstString cstString)
        {
            _buffer.Push(cstString.Data);
        }

        public override void Apply(CstStateName cstStateName)
        {
            _buffer.Push(cstStateName.Name);
        }

        public override void Apply(CstStatesList cstStatesList)
        {
            throw new System.NotImplementedException();
        }

        public override void Apply(CstTransition cstTransition)
        {
            var transitionOptions = new TransitionOptions();
            
            if (cstTransition.IsEpsilon)
            {
                cstTransition.StartStateName.Visit(this);
                cstTransition.EndStateName.Visit(this);

                transitionOptions.IsEpsilon = true;
                transitionOptions.EndState = (string) _buffer.Pop();
                transitionOptions.StartState = (string) _buffer.Pop();
            }

            else
            {
                cstTransition.StartStateName.Visit(this);
                cstTransition.Token.Visit(this);
                cstTransition.EndStateName.Visit(this);

                transitionOptions.IsEpsilon = false;
                transitionOptions.EndState = (string) _buffer.Pop();
                transitionOptions.Token = (string) _buffer.Pop();
                transitionOptions.StartState = (string) _buffer.Pop();
            }
            
            _buffer.Push(transitionOptions);
        }

        public override void Apply(CstDeclaration cstDeclaration)
        {
            var stateMachineArchOptions = new SMOptions();
            
            cstDeclaration.Identity.Visit(this);
            
            if (cstDeclaration.States != null)
            {
                stateMachineArchOptions.States = new List<string>();
                
                foreach (var state in cstDeclaration.States)
                {
                    state.Visit(this);
                    stateMachineArchOptions.States.Add((string) _buffer.Pop());
                }
            }

            
            stateMachineArchOptions.FinalStates = new List<string>();
            
            foreach (var final in cstDeclaration.Finals)
            {
                final.Visit(this);
                stateMachineArchOptions.FinalStates.Add((string) _buffer.Pop());
            }


            cstDeclaration.InitialStateName.Visit(this);
            stateMachineArchOptions.InitialState = (string) _buffer.Pop();
            
            
            stateMachineArchOptions.Transitions = new List<TransitionOptions>();

            foreach (var transition in cstDeclaration.Trancitions)
            {
                transition.Visit(this);
                stateMachineArchOptions.Transitions.Add((TransitionOptions) _buffer.Pop());
            }

            var identity = (string) _buffer.Pop();
            stateMachineArchOptions.MachineId = identity;
            
            _buffer.Push(stateMachineArchOptions);
        }

        public override void Apply(CstAsset cstAsset)
        {
            cstAsset.Identity.Visit(this); // What machine own it

            var asset = new Asset {Text = cstAsset.String, ExpectedResult = true};
            _buffer.Push(asset);
        }

        public override void Apply(CstDsl cstDsl)
        {
            foreach (var declaration in cstDsl.Declarations)
            {
                declaration.Visit(this);

                var options = new Options {Arch = ((SMOptions) _buffer.Pop()), Assets = new List<Asset>()};
                _options.Add(options.Arch.MachineId, options);
            }
            
            foreach (var assetCst in cstDsl.Assets)
            {
                assetCst.Visit(this);
                
                var asset = (Asset) _buffer.Pop();
                var identity = (string) _buffer.Pop();
                
                _options[identity].Assets.Add(asset);
            }
            
            foreach (var (_, options) in _options)
            {
                options.Assets = options.Assets.Any() ? options.Assets : null;
            }
        }
    }
}