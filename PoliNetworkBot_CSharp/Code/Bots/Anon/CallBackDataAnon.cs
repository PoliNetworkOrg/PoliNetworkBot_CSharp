#region

using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon;

internal class CallBackDataAnon : CallbackGenericData
{
    public long? AuthorId;
    public bool? FromTelegram;
    public long? Identity;
    public string? LangUser;
    public int? MessageIdReplyTo;
    public long? MessageIdUser;
    public string? Username;

    public CallBackDataAnon(List<CallbackOption> options, Action<CallbackGenericData> runAfterSelection) : base(options,
        runAfterSelection)
    {
    }

    public static string? ResultToString(ResultQueueEnum? item2)
    {
        return item2 switch
        {
            ResultQueueEnum.APPROVED_MAIN => "a",
            ResultQueueEnum.GO_TO_UNCENSORED => "u",
            ResultQueueEnum.DELETE => "d",
            _ => null
        };
    }

    internal ResultQueueEnum? GetResultEnum()
    {
        var callbackOption = Options[SelectedAnswer];
        var callbackOptionValue = callbackOption.value;
        return (ResultQueueEnum?)callbackOptionValue;
    }
}