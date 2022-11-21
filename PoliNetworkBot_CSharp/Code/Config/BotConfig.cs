#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Config;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class BotConfig
{
    // ReSharper disable once InconsistentNaming
    public List<BotInfoAbstract>? bots;

    public static bool GetConfig(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        return e != null && ConfigUtil.GetConfig(e.Message.From?.Id, e.Message.From?.Username, sender,
            e.Message.From?.LanguageCode,
            e.Message.Chat.Type);
    }

    public static Task GetConfig2(MessageEventArgs? arg1, TelegramBotAbstract? arg2, string[]? arg3)
    {
        GetConfig(arg1, arg2);
        return Task.CompletedTask;
    }
}