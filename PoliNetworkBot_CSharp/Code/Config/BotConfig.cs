#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Config;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class BotConfig
{
    // ReSharper disable once InconsistentNaming
    public List<BotInfoAbstract>? bots;

    public static CommandExecutionState GetConfig(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (e == null)
            return CommandExecutionState.UNMET_CONDITIONS;

        var messageFrom = e.Message.From;
        var config = ConfigUtil.GetConfig(messageFrom?.Id, messageFrom?.Username, sender,
            messageFrom?.LanguageCode,
            e.Message.Chat.Type);
        return config
            ? CommandExecutionState.SUCCESSFUL
            : CommandExecutionState.UNMET_CONDITIONS;
    }

    public static Task GetConfig2(MessageEventArgs? arg1, TelegramBotAbstract? arg2, string[]? arg3)
    {
        GetConfig(arg1, arg2);
        return Task.CompletedTask;
    }

    public static Task GetDbConfig(MessageEventArgs? arg1, TelegramBotAbstract? arg2)
    {
        ConfigUtil.GetDbConfig(arg1, arg2);
        return Task.CompletedTask;
    }
}