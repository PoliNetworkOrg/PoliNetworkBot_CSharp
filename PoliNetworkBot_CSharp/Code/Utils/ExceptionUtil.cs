using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class ExceptionUtil
{
    public static List<Exception?> Concat(Exception? ex, ValueWithException<DateTime?>? d1)
    {
        var r = new List<Exception?>
        {
            ex
        };
        var d2 = d1?.GetExceptions();
        if (d1 == null || !d1.ContainsExceptions())
            return r;
        if (d2 != null)
            r.AddRange(d2);

        return r;
    }

}