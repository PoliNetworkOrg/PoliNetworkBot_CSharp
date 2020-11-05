using System;
using TeleSharp.TL;

namespace PoliNetworkBot_CSharp.Code.Objects.TmpResults
{
    internal class ResultF1
    {
        public readonly bool? returnobject;
        public readonly int? idMessageAdded;
        public readonly TLAbsUpdates r;
        public readonly Tuple<TLAbsUpdates, Exception> r2;

    

        public ResultF1(bool? returnobject, int? idMessageAdded, TLAbsUpdates r, Tuple<TLAbsUpdates, Exception> r2) 
        {
            this.returnobject = returnobject;
            this.idMessageAdded = idMessageAdded;
            this.r = r;
            this.r2 = r2;
        }
    }
}