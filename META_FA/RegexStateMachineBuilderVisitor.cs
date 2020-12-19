using System.Collections.Generic;
using Regex_Parser.CST;
using Regex_Parser.Visitors.CST;
using StateMachineLib.StateMachine;

namespace META_FA
{
    public class RegexStateMachineBuilderVisitor : CSTVisitor
    {
        private readonly List<State> _startNodes = new List<State>();
        private readonly List<State> _endNodes = new List<State>();
        private readonly List<Transition> _transitions = new List<Transition>();

        private readonly MachineNonDetermined _machine;

        public MachineNonDetermined GetResult()
        {
            var finalNode = new State(isFinal: true);
            var transition = new Transition(_endNodes[^1], finalNode);

            _machine.AddState(finalNode);
            _machine.AddTransition(transition);

            _endNodes.Add(finalNode);
            _transitions.Add(transition);

            _machine.Init(_startNodes[^1].Id);
            return _machine.RenameToNormalNames();
        }

        public RegexStateMachineBuilderVisitor()
        {
            _machine = new MachineNonDetermined();
        }

        public override void Apply(RegexVariant regexVariant)
        {
            var q0 = new State();
            var qk = new State();

            var startsNodes = new List<State>();
            var endNodes = new List<State>();

            foreach (var child in regexVariant.Children)
            {
                child.Visit(this);
                startsNodes.Add(_startNodes[^1]);
                endNodes.Add(_endNodes[^1]);
            }

            _machine.AddStateRange(new List<State> {q0, qk});
            _startNodes.Add(q0);
            _endNodes.Add(qk);

            var transitions = new List<Transition>();

            for (var i = 0; i < startsNodes.Count; ++i)
            {
                transitions.AddRange(new List<Transition>
                {
                    new Transition(q0, startsNodes[i]),
                    new Transition(endNodes[i], qk),
                });
            }

            _machine.AddTransitionRange(transitions);
            _transitions.AddRange(transitions);
        }

        public override void Apply(RegexString regexString)
        {
            var q0 = new State();
            var qk = new State();

            var startsNodes = new List<State>();
            var endNodes = new List<State>();

            foreach (var child in regexString.Children)
            {
                child.Visit(this);
                startsNodes.Add(_startNodes[^1]);
                endNodes.Add(_endNodes[^1]);
            }

            _machine.AddStateRange(new List<State> {q0, qk});
            _startNodes.Add(q0);
            _endNodes.Add(qk);

            var transitions = new List<Transition>
            {
                new Transition(q0, startsNodes[0]),
                new Transition(endNodes[^1], qk),
            };

            for (var i = 1; i < startsNodes.Count; ++i)
            {
                transitions.Add(new Transition(endNodes[i - 1], startsNodes[i]));
            }

            _machine.AddTransitionRange(transitions);
            _transitions.AddRange(transitions);
        }

        public override void Apply(RegexQuantifierZeroOrInfinity regexQuantifierZeroOrInfinity)
        {
            regexQuantifierZeroOrInfinity.Child.Visit(this);

            var q0 = new State();
            var qk = new State();

            var startState = _startNodes[^1];
            var endState = _endNodes[^1];

            _machine.AddStateRange(new List<State> {q0, qk});
            _startNodes.Add(q0);
            _endNodes.Add(qk);

            var transitions = new List<Transition>
            {
                new Transition(endState, startState),
                new Transition(q0, startState),
                new Transition(endState, qk),
                new Transition(q0, qk)
            };

            _machine.AddTransitionRange(transitions);
            _transitions.AddRange(transitions);
        }

        public override void Apply(RegexQuantifierOneOrInfinity regexQuantifierOneOrInfinity)
        {
            var q0 = new State();
            var qk = new State();

            regexQuantifierOneOrInfinity.Child.Visit(this);
            var q1 = _startNodes[^1];
            var q2 = _endNodes[^1];

            new RegexQuantifierZeroOrInfinity(regexQuantifierOneOrInfinity.Child).Visit(this);
            var q3 = _startNodes[^1];
            var q4 = _endNodes[^1];

            _machine.AddStateRange(new List<State> {q0, qk});
            _startNodes.Add(q0);
            _endNodes.Add(qk);

            var transitions = new List<Transition>
            {
                new Transition(q0, q1),
                new Transition(q2, q3),
                new Transition(q4, qk),
            };

            _machine.AddTransitionRange(transitions);
            _transitions.AddRange(transitions);
        }

        public override void Apply(RegexQuantifierMaybe regexQuantifierMaybe)
        {
            regexQuantifierMaybe.Child.Visit(this);

            var q0 = _startNodes[^1];
            var qk = _endNodes[^1];

            var q1 = new State();

            _machine.AddState(q1);
            _startNodes.Add(q1);

            var transitions = new List<Transition>
            {
                new Transition(q1, q0),
                new Transition(q1, qk)
            };

            _machine.AddTransitionRange(transitions);
            _transitions.AddRange(transitions);
        }

        public override void Apply(RegexSymbol regexSymbol)
        {
            var q0 = new State();
            var qk = new State();

            _machine.AddStateRange(new List<State> {q0, qk});
            _startNodes.Add(q0);
            _endNodes.Add(qk);

            var transition = new Transition(q0, regexSymbol.Token, qk);
            _machine.AddTransition(transition);
            _transitions.Add(transition);
        }
    }
}