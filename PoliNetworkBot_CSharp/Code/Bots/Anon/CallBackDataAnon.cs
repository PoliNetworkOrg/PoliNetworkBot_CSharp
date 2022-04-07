#region

using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;
using System;
using System.Collections.Generic;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon;

internal class CallBackDataAnon : CallbackGenericData
{
    public long? authorId;
    public bool? from_telegram;
    public long? identity;
    public string langUser;
    public int? messageIdReplyTo;
    public long? messageIdUser;
    public string username;

    public CallBackDataAnon(List<CallbackOption> options, Action<CallbackGenericData> runAfterSelection) : base(options,
        runAfterSelection)
    {
    }

    public static string ResultToString(ResultQueueEnum? item2)
    {
        return item2 switch
        {
            ResultQueueEnum.APPROVED_MAIN => "a",
            ResultQueueEnum.GO_TO_UNCENSORED => "u",
            ResultQueueEnum.DELETE => "d",
            _ => null
        };
    }

    internal ResultQueueEnum GetResultEnum()
    {
        return (ResultQueueEnum)Options[SelectedAnswer].value;
    }
}