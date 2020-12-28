using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DSL_Parser.CST;

namespace META_FA.Exceptions
{
    public class BuiltInFunctionArgumentsNumberException : CoreException
    {
        public override string Message { get; }

        public BuiltInFunctionArgumentsNumberException(string funcName, IList<CstFuncArg> args, int expectedCount)
        {
            Message = $"Func[{funcName}] args: {args}, expectedCount: {expectedCount}";
        }

        public BuiltInFunctionArgumentsNumberException()
        {
        }

        public BuiltInFunctionArgumentsNumberException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BuiltInFunctionArgumentsNumberException(string? message) : base(message)
        {
        }

        public BuiltInFunctionArgumentsNumberException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}