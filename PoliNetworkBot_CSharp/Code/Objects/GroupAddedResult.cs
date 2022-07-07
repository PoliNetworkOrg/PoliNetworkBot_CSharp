using System;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Objects;

public class GroupAddedResult
{
    private bool _b;
    private List<Exception> _exceptions;
    public GroupAddedResult(bool d2, List<Exception> exceptions)
    {
        _b = d2;
        _exceptions = exceptions;
    }
}