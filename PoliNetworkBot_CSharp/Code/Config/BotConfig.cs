#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
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

        var eMessage = e.Message;
        var eMessageFrom = eMessage.From;
        var eMessageChat = eMessage.Chat;

        var config = ConfigUtil.GetConfig(eMessageFrom?.Id, eMessageFrom?.Username, sender,
            eMessageFrom?.LanguageCode,
            eMessageChat.Type, eMessage.MessageThreadId);
        return config
            ? CommandExecutionState.SUCCESSFUL
            : CommandExecutionState.UNMET_CONDITIONS;
    }

    public static void GetConfig2(ActionFuncGenericParams actionFuncGenericParams)
    {
        GetConfig(actionFuncGenericParams.MessageEventArgs, actionFuncGenericParams.TelegramBotAbstract);
    }

    public static void GetDbConfig(ActionFuncGenericParams actionFuncGenericParams)
    {
        ConfigUtil.GetDbConfig(actionFuncGenericParams.MessageEventArgs, actionFuncGenericParams.TelegramBotAbstract);
    }
}