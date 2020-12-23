using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using StateMachineLib.Options;
using StateMachineLib.StateMachine.Exceptions;

namespace StateMachineLib.StateMachine
{
    public abstract class Machine
    {
        public Guid Id { get; } = Guid.NewGuid();
        protected readonly List<State> States = new List<State>();
        protected readonly List<Transition> Transitions = new List<Transition>();
        public State InitialState { get; protected set; }
        public abstract MachineType Type { get; }
        
        public bool IsInited { get; private set; }
        protected bool IsThereAnyEpsilonTransition => Transitions.Any(transition => transition.IsEpsilon);
        protected bool IsThereAnyMultiVariantTokenTransition => Transitions
                                       .GroupBy(tr => new {tr.StartState, tr.Token})
                                       .Any(gr => gr.Count() > 1);

        public bool IsUndetermined => IsThereAnyEpsilonTransition || IsThereAnyMultiVariantTokenTransition;

        public IEnumerable<State> GetStates()
        {
            return new ReadOnlyCollection<State>(States);
        }
        
        public IEnumerable<Transition> GetTransitions()
        {
            return new ReadOnlyCollection<Transition>(Transitions);
        }
        
        public void AddState(State newState)
        {
            var foundState = States.Find(state => Equals(state, newState));
            if (foundState != null)
            {
                throw new DuplicateStateException(newState, this);
            }
            States.Add(newState);
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
            Transitions.Add(newTransition);
        }

        public void AddTransitionRange(IEnumerable<Transition> newTransitions)
        {
            foreach (var transition in newTransitions)
            {
                AddTransition(transition);
            }
        }

        public void RemoveState(State node)
        {
            IsInited = false;
            States.Remove(node);
        }

        public void RemoveTransition(Transition transition)
        {
            IsInited = false;
            Transitions.Remove(transition);
        }
        
        public void Init(string initialStateId)
        {
            if (IsInited) return;
            
            var initialState = States.Find(state => state.Id == initialStateId);
            InitialState = initialState ?? throw new InitialStateIsNullException(initialStateId, this);

            if (!States.Any(state => state.IsFinal))
            {
                throw new NoAnyFinalStateException(this);
            }

            var oblivionWayTransitions = Transitions
                .Select(transition => States.Find(state => state.Id == transition.EndState.Id))
                .Any(foundState => foundState == null);
            if (oblivionWayTransitions)
            {
                throw new OblivionWayTransitionsException(this);
            }

            var unreachableStates = States
                .Where(state => state.Id != initialStateId)
                .Select(state => Transitions.Find(transition => Equals(transition.EndState, state) && !Equals(transition.StartState, state)))
                .Any(foundTransition => foundTransition == null);
            if (unreachableStates)
            {
                Console.WriteLine($"[Warning] There is some unreachable states into machine: {Id}");
                var isReachableFinalState = States
                    .Where(state => state.IsFinal && state.Id != initialStateId)
                    .Select(state => Transitions.Find(transition => Equals(transition.EndState, state)))
                    .Any(foundTransition => foundTransition != null);
                if (!isReachableFinalState)
                {
                    throw new NoAnyReachableFinalStateException(this);
                }
            }

            IsInited = true;
        }

        public bool Run(string text)
        {
            if (!IsInited) throw new UninitedMachineRunException(this);
            return DoStep(text, InitialState);
        }

        protected abstract bool DoStep(string text, State currentState);
        
        protected abstract void PreAddTransitionCheck(Transition newTransition);
        
        public static Machine GetFromOptions(SMOptions options)
        {
            Machine stateMachine;
            if(options.Transitions.Any(tr => tr.IsEpsilon)
            || options.Transitions
                .GroupBy(tr => tr.StartState + tr.Token)
                .Any(gr => gr.Count() > 1))
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
            var transitions = Transitions
                .Where(transition => transition.IsEpsilon)
                .Select(transition => new TransitionOptions {
                    StartState = transition.StartState.Id,
                    EndState = transition.EndState.Id,
                    IsEpsilon = true
                }).ToList();
            
            transitions.AddRange(Transitions.Where(transition => !transition.IsEpsilon)
                .Select(transition => new TransitionOptions {
                    StartState = transition.StartState.Id,
                    EndState = transition.EndState.Id,
                    Token = transition.Token
                }));
            
            return new SMOptions {
                MachineId = Id.ToString(),
                InitialState = InitialState.Id,
                FinalStates = States.Where(state => state.IsFinal).Select(state => state.Id).ToList(),
                Transitions = transitions
            };
        }

        public abstract Machine Minimize();
        public abstract MachineDetermined Determine(bool verbose = false);

        public Machine RenameToNormalNames(string startsWith = null)
        {
            return Type switch
            {
                MachineType.Determined => ((MachineDetermined)this).RenameToNormalNames(startsWith),
                MachineType.NonDetermined => ((MachineNonDetermined)this).RenameToNormalNames(startsWith),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}