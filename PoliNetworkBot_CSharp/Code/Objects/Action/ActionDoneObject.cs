using System;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

public class ActionDoneObject
{
    private readonly SpamType? _spamType;
    public readonly ActionDoneEnum ActionDoneEnum;
    public readonly bool? Done;

    public ActionDoneObject(ActionDoneEnum actionDone, bool? done, SpamType? spamType)
    {
        ActionDoneEnum = actionDone;
        Done = done;
        _spamType = spamType;
    }

    public override bool Equals(object? obj)
    {
        return obj is ActionDoneObject actionDoneObject && Equals(actionDoneObject);
    }

    private bool Equals(ActionDoneObject other)
    {
        return ActionDoneEnum == other.ActionDoneEnum && Done == other.Done && _spamType == other._spamType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)ActionDoneEnum, Done, _spamType);
    }

    public override string ToString()
    {
        return ActionDoneEnum + " | " + Done + " | " + _spamType;
    }
}