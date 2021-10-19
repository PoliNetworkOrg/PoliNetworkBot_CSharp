using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Utils;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class SuccessWithException
    {
        private readonly List<Exception> ex;
        private readonly bool success;

        public SuccessWithException(bool v)
        {
            success = v;
        }

        public SuccessWithException(bool v, Exception e2)
        {
            success = v;
            ex = new List<Exception> {e2};
        }

        public SuccessWithException(bool v, List<Exception> e2)
        {
            success = v;
            ex = e2;
        }

        internal bool IsSuccess()
        {
            return success;
        }

        internal List<Exception> GetExceptions()
        {
            return ex;
        }

        internal bool ContainsExceptions()
        {
            return ex != null && ex.Count > 0;
        }

        internal ExceptionNumbered GetFirstException()
        {
            if (ContainsExceptions())
            {
                var ex2 = ex[0];
                return new ExceptionNumbered(ex2);
            }

            return null;
        }
    }
}