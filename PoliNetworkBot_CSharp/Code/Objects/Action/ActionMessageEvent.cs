#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

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