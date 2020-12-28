using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSL_Parser.CST;
using META_FA.BuiltInFuncs.Exceptions;
using StateMachineLib.StateMachine;

namespace META_FA.BuiltInFuncs
{
    public partial class BuiltInFunction
    {
        public string Name { get; }
        public ReadOnlyCollection<string> ArgsTypes { get; }
        private Action<List<CstFuncArg>> BusinessLogic { get; }

        public BuiltInFunction(string name, List<string> argsTypes, Action<List<CstFuncArg>> businessLogic)
        {
            Name = name;
            ArgsTypes = new ReadOnlyCollection<string>(argsTypes);
            BusinessLogic = businessLogic;
        }


        public void Execute(List<CstFuncArg> args)
        {
            CheckArgsNumber(args);
            CheckArgsTypes(args);
            BusinessLogic(args);
        }
        

        public void CheckArgsNumber(List<CstFuncArg> args)
        {
            if (args.Count != ArgsTypes.Count)
                throw new BuiltInFunctionArgumentsNumberException(Name, args, ArgsTypes.Count);
        }

        public void CheckArgsTypes(List<CstFuncArg> args)
        {
            var argsAndTypes = args.Zip(ArgsTypes, (arg, type) => new {Arg = arg, ExcpectedType = type});

            foreach (var ant in argsAndTypes)
            {
                if (ant.Arg.Type != ant.ExcpectedType)
                    throw new BuiltInFunctionArgumentTypeException(Name, ant.Arg, ant.ExcpectedType);
            }
        }
    }
}