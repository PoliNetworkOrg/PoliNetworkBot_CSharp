#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CallbackGenericData
{
    internal TelegramBotAbstract? Bot;
    internal CallbackQuery? CallBackQueryFromTelegram;
    public string? Id;
    internal DateTime? InsertedTime;
    internal MessageSentResult? MessageSent;
    public List<CallbackOption> Options;
    public Action<CallbackGenericData> RunAfterSelection;
    internal int SelectedAnswer;

    public CallbackGenericData(List<CallbackOption> options, Action<CallbackGenericData> runAfterSelection)
    {
        Options = options;
        RunAfterSelection = runAfterSelection;
    }

    internal bool IsExpired()
    {
        if (InsertedTime == null)
            return false;

        return InsertedTime.Value.AddDays(7) <= DateTime.Now;
    }
}