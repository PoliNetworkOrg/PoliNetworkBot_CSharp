#region

using System;
using System.Collections.Generic;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

internal class ValueWithException<T>
{
    private readonly SuccessWithException SuccessWithException;
    private readonly T value;

    public ValueWithException(T d2, Exception? p)
    {
        SuccessWithException = new SuccessWithException(d2 != null && p == null, p);
        value = d2;
    }

    internal List<Exception?>? GetExceptions()
    {
        return SuccessWithException.GetExceptions();
    }

    internal bool ContainsExceptions()
    {
        return SuccessWithException.ContainsExceptions();
    }

    internal T GetValue()
    {
        return value;
    }

    internal bool HasValue()
    {
        return value != null;
    }
}