﻿using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class SuccessWithException
    {
        private bool success;
        private List<Exception> ex;

        public SuccessWithException(bool v)
        {
            this.success = v;
        }

        public SuccessWithException(bool v, Exception e2)
        {
            this.success = v;
            this.ex = new List<Exception> (){ e2 };
        }


        public SuccessWithException(bool v, List<Exception> e2)
        {
            this.success = v;
            this.ex = e2;
        }



        internal bool isSuccess()
        {
            return this.success;
        }

        internal List<Exception> GetExceptions()
        {
            return this.ex;
        }

        internal bool ContainsExceptions()
        {
            return this.ex != null && this.ex.Count > 0;
        }

        internal ExceptionNumbered GetFirstException()
        {
            if (this.ContainsExceptions())
            {
                var ex2 =  this.ex[0];
                return new ExceptionNumbered(ex2);
            }
            return null;
        }
    } 
}