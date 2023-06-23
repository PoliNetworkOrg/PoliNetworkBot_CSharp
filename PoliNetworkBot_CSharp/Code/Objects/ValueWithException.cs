#region

using System;
using System.Collections.Generic;
using SampleNuGet.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class ValueWithException<T>
{
    private readonly SuccessWithException _successWithException;
    private readonly T _value;

    public ValueWithException(T d2, Exception? p)
    {
        _successWithException = new SuccessWithException(d2 != null && p == null, p);
        _value = d2;
    }

    internal List<Exception?>? GetExceptions()
    {
        return _successWithException.GetExceptions();
    }

    internal bool ContainsExceptions()
    {
        return _successWithException.ContainsExceptions();
    }

    internal T GetValue()
    {
        return _value;
    }

    internal bool HasValue()
    {
        return _value != null;
    }
}