#region

using System;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data;

public class ActionMessageEvent
{
    private readonly Action<object?, MessageEventArgs?>? _action;

    public ActionMessageEvent(Action<object?, MessageEventArgs?>? action)
    {
        _action = action;
    }

    public Action<object?, MessageEventArgs?>? GetAction()
    {
        return _action;
    }
}