using System;
using System.Collections.Generic;
using System.Linq;
using META_FA.Options;
using META_FA.StateMachine.Exceptions;

namespace META_FA.StateMachine
{
    public abstract class Machine
    {
        public Guid Id { get; } = Guid.NewGuid();
        protected readonly List<State> _states = new List<State>();
        protected readonly List<Transition> _transitions = new List<Transition>();
        protected State _initialState;
        public abstract MachineType Type { get; }
        protected bool _inited = false;

        public void AddState(State newState)
        {
            var foundState = _states.Find(state => Equals(state, newState));
            if (foundState != null)
            {
                throw new DuplicateStateException(newState, this);
            }
            _states.Add(newState);
        }

        public void AddTransition(Transition newTransition)
        {
            PreAddTransitionCheck(newTransition);
            _transitions.Add(newTransition);
        }
        
        public void Init(string initialStateId)
        {
            if (_inited) return;
            
            var initialState = _states.Find(state => state.Id == initialStateId);
            _initialState = initialState ?? throw new InitialStateIsNullException(initialStateId, this);

            if (!_states.Any(state => state.IsFinal))
            {
                throw new NoAnyFinalStateException(this);
            }

            var oblivionWayTransitions = _transitions
                .Select(transition => _states.Find(state => state.Id == transition.EndState.Id))
                .Any(foundState => foundState == null);
            if (oblivionWayTransitions)
            {
                throw new OblivionWayTransitionsException(this);
            }

            var unreachableStates = _states
                .Where(state => state.Id != initialStateId)
                .Select(state => _transitions.Find(transition => transition.EndState.Id == state.Id))
                .Any(foundTransition => foundTransition == null);
            if (unreachableStates)
            {
                Console.WriteLine($"[Warning] There is some unreachable states into machine: {Id}");
                var noAnyReachableFinalState = _states
                    .Where(state => state.IsFinal && state.Id != initialStateId)
                    .Select(state => _transitions.Find(transition => Equals(transition.EndState, state)))
                    .Any(foundTransition => foundTransition == null);
                if (noAnyReachableFinalState)
                {
                    throw new NoAnyReachableFinalStateException(this);
                }
            }

            _inited = true;
        }

        public bool Run(string text)
        {
            if (!_inited) throw new UninitedMachineRunException(this);
            return DoStep(text, _initialState);
        }
        
        private bool DoStep(string text, State currentState)
        {
            // [TODO] Think about ε-transitions
            if (currentState.IsFinal && text == "")
                return true;
            
            var token = text[0].ToString();
            
            var ways = _transitions.FindAll(transition
                => Equals(transition.StartState, currentState)
                && (transition.Token == token || transition.IsEpsilon));

            return ways.Any(way => DoStep(text.Substring(1), way.EndState));
        }

        protected abstract void PreAddTransitionCheck(Transition newTransition);
        
        public static Machine GetFromOptions(SMOptions options)
        {
            Machine stateMachine;
            if(options.Transitions.Any(tr => tr.IsEpsilon)
            || options.Transitions
                .GroupBy(tr => tr.StartState + tr.Token)
                .Any(@gr => @gr.Count() > 1))
            {
                stateMachine = new MachineNonDetermined();
            }
            else
            {
                stateMachine = new MachineDetermined();
            }
            
            var statesNames = options.GetStates();
            var statesDict = new Dictionary<string, State>();
            foreach (var stateName in statesNames)
            {
                var state = new State(stateName, options.FinalStates.Contains(stateName));
                statesDict.Add(stateName, state);
                stateMachine.AddState(state);
            }

            foreach (var transitionOptions in options.Transitions)
            {
                if (transitionOptions.IsEpsilon)
                {
                    stateMachine.AddTransition(new Transition(
                        statesDict[transitionOptions.StartState],
                        statesDict[transitionOptions.EndState]
                    ));
                }
                
                else
                {
                    stateMachine.AddTransition(new Transition(
                        statesDict[transitionOptions.StartState],
                        transitionOptions.Token,
                        statesDict[transitionOptions.EndState]
                    ));
                }
            }
            
            stateMachine.Init(options.InitialState);
            
            return stateMachine;
        }

        public SMOptions ToOptions()
        {
            var transitions = _transitions
                .Where(transition => transition.IsEpsilon)
                .Select(transition => new TransitionOptions {
                    StartState = transition.StartState.Id,
                    EndState = transition.EndState.Id,
                    IsEpsilon = true
                }).ToList();
            
            transitions.AddRange(_transitions.Where(transition => !transition.IsEpsilon)
                .Select(transition => new TransitionOptions {
                    StartState = transition.StartState.Id,
                    EndState = transition.EndState.Id,
                    Token = transition.Token
                }));
            
            return new SMOptions {
                InitialState = _initialState.Id,
                FinalStates = _states.Where(state => state.IsFinal).Select(state => state.Id).ToList(),
                Transitions = transitions
            };
        }

        public abstract Machine Minimize();
        public abstract MachineDetermined Determine();
    }
}