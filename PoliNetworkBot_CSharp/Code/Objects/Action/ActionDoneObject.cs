using System;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

public class ActionDoneObject
{
    private readonly ActionDoneEnum _actionDoneEnum;
    private readonly bool? _done;
    private SpamType? _spamType;

    public ActionDoneObject(ActionDoneEnum actionDone, bool? done, SpamType? spamType)
    {
        _actionDoneEnum = actionDone;
        _done = done;
        _spamType = spamType;
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
        return _actionDoneEnum + " | " + _done + " | " + _spamType;
    }

    public void SetSpamType(SpamType resultItem1)
    {
        this._spamType = resultItem1;
    }
}