#region

using System;
using System.Collections.Generic;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class SuccessWithException
{
    private readonly List<Exception?>? ex;
    private readonly bool success;

    public SuccessWithException(bool v)
    {
        success = v;
    }

    public SuccessWithException(bool v, Exception? e2)
    {
        success = v;
        ex = new List<Exception?> { e2 };
    }

    public SuccessWithException(bool v, List<Exception?>? e2)
    {
        success = v;
        ex = e2;
    }

    internal bool IsSuccess()
    {
        return success;
    }

    internal List<Exception?>? GetExceptions()
    {
        return ex;
    }

    internal bool ContainsExceptions()
    {
        return ex is { Count: > 0 };
    }

    internal ExceptionNumbered? GetFirstException()
    {
        if (!ContainsExceptions()) return null;
        if (ex == null) return null;
        var ex2 = ex[0];
        return new ExceptionNumbered(ex2);
    }
}