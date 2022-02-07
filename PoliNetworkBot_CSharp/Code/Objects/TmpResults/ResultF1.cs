#region

using System;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TmpResults
{
    internal class ResultF1
    {
        public readonly long? idMessageAdded;
        public readonly TLAbsUpdates r;
        public readonly Tuple<TLAbsUpdates, Exception> r2;
        public readonly bool? returnobject;

        public ResultF1(bool? returnobject, long? idMessageAdded, TLAbsUpdates r, Tuple<TLAbsUpdates, Exception> r2)
        {
            this.returnobject = returnobject;
            this.idMessageAdded = idMessageAdded;
            this.r = r;
            this.r2 = r2;
        }
    }
}