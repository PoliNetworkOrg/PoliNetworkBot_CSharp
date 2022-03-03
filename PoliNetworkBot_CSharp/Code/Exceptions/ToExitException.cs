#region

using System;
using System.Runtime.Serialization;

#endregion

namespace PoliNetworkBot_CSharp.Code.Exceptions
{
    [Serializable]
    internal class ToExitException : Exception
    {
        public ToExitException()
        {
        }

        public ToExitException(string message) : base(message)
        {
        }

        public ToExitException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ToExitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}