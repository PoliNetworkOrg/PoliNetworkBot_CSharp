using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

public class ActionDoneObject
{
    public ActionDoneEnum ActionDoneEnum;
    public bool? done;
    public SpamType? SpamType;

    public ActionDoneObject(ActionDoneEnum actionDone, bool? done, SpamType? spamType)
    {
        this.ActionDoneEnum = actionDone;
        this.done = done;
        this.SpamType = spamType;
    }
}