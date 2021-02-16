using System;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class ValueWithException<T>
    {
        private readonly SuccessWithException SuccessWithException;
        private readonly T value;

        public ValueWithException(T d2, Exception p)
        {
            this.SuccessWithException = new SuccessWithException(d2 != null && p == null, p);
            this.value = d2;
        }

        internal List<Exception> GetExceptions()
        {
            return this.SuccessWithException.GetExceptions();
        }

        internal bool ContainsExceptions()
        {
            return this.SuccessWithException.ContainsExceptions();
        }

        internal T GetValue()
        {
            return this.value;
        }

        internal bool HasValue()
        {
            return this.value != null;
        }
    }
}