#region

using System;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TmpResults;

internal class ResultF1
{
    public readonly long? IdMessageAdded;
    public readonly TLAbsUpdates? R;
    public readonly Tuple<TLAbsUpdates?, Exception?>? R2;
    public readonly bool? ReturnObject;

    public ResultF1(bool? returnObject, long? idMessageAdded, TLAbsUpdates? r, Tuple<TLAbsUpdates?, Exception?>? r2)
    {
        ReturnObject = returnObject;
        IdMessageAdded = idMessageAdded;
        R = r;
        R2 = r2;
    }
}