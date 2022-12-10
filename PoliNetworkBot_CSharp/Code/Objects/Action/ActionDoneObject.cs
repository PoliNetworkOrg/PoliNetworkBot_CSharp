using System;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

public class ActionDoneObject
{
    private readonly ActionDoneEnum _actionDoneEnum;
    private readonly bool? _done;
    private readonly SpamType? _spamType;

    public ActionDoneObject(ActionDoneEnum actionDone, bool? done, SpamType? spamType)
    {
        this._actionDoneEnum = actionDone;
        this._done = done;
        this._spamType = spamType;
    }

    public override bool Equals(object? obj)
    {
        return obj is ActionDoneObject actionDoneObject && Equals(actionDoneObject);
    }

    private bool Equals(ActionDoneObject other)
    {
        return _actionDoneEnum == other._actionDoneEnum && _done == other._done && _spamType == other._spamType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)_actionDoneEnum, _done, _spamType);
    }

    public override string ToString()
    {
        return this._actionDoneEnum + " | " + this._done + " | " + this._spamType;
    }
}