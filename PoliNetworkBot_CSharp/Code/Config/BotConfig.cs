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

<<<<<<< HEAD
    public static async Task<CommandExecutionState> GetConfig(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (e == null)
            return CommandExecutionState.UNMET_CONDITIONS;
        return await ConfigUtil.GetConfig(e.Message.From?.Id, e.Message.From?.Username, sender,
=======
    public static bool GetConfig(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        return e != null && ConfigUtil.GetConfig(e.Message.From?.Id, e.Message.From?.Username, sender,
>>>>>>> 64c252f368a18ccab709c2e01f0d14fcf4812465
            e.Message.From?.LanguageCode,
            e.Message.Chat.Type)
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