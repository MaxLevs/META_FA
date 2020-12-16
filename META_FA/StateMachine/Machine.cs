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
        protected bool _inited;

        public void AddState(State newState)
        {
            var foundState = _states.Find(state => Equals(state, newState));
            if (foundState != null)
            {
                throw new DuplicateStateException(newState, this);
            }
            _states.Add(newState);
        }

        public void AddStateRange(IEnumerable<State> newStates)
        {
            foreach (var state in newStates)
            {
                AddState(state);
            }
        }

        public void AddTransition(Transition newTransition)
        {
            PreAddTransitionCheck(newTransition);
            _transitions.Add(newTransition);
        }

        public void AddTransitionRange(IEnumerable<Transition> newTransitions)
        {
            foreach (var transition in newTransitions)
            {
                AddTransition(transition);
            }
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
                var isReachableFinalState = _states
                    .Where(state => state.IsFinal && state.Id != initialStateId)
                    .Select(state => _transitions.Find(transition => Equals(transition.EndState, state)))
                    .Any(foundTransition => foundTransition != null);
                if (!isReachableFinalState)
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

        protected abstract bool DoStep(string text, State currentState);
        
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

        public Machine RenameToNormalNames()
        {
            return Type switch
            {
                MachineType.Determined => ((MachineDetermined)this).RenameToNormalNames(),
                MachineType.NonDetermined => ((MachineNonDetermined)this).RenameToNormalNames(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}