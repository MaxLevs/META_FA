using System;
using System.Runtime.Serialization;

namespace META_FA.BuiltInFuncs.Exceptions
{
    public abstract class CoreException : Exception
    {
        protected CoreException()
        {
        }

        protected CoreException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected CoreException(string? message) : base(message)
        {
        }

        protected CoreException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}