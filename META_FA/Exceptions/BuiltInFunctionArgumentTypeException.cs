using System;
using System.Runtime.Serialization;
using DSL_Parser.CST;

namespace META_FA.Exceptions
{
    public class BuiltInFunctionArgumentTypeException : CoreException
    {
        public override string Message { get; }

        public BuiltInFunctionArgumentTypeException()
        {
            Message = "There is some error with argument type";
        }

        public BuiltInFunctionArgumentTypeException(string funcName, CstFuncArg arg, string expectedType)
        {
            Message = $"Func[{funcName}]: arg: {arg.Type} must be {expectedType}";
        }

        public BuiltInFunctionArgumentTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BuiltInFunctionArgumentTypeException(string message) : base(message)
        {
        }

        public BuiltInFunctionArgumentTypeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}