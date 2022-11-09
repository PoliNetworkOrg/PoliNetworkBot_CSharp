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

    public static async Task<CommandExecutionState> GetConfig(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (e == null)
            return CommandExecutionState.UNMET_CONDITIONS;
        return await ConfigUtil.GetConfig(e.Message.From?.Id, e.Message.From?.Username, sender,
            e.Message.From?.LanguageCode,
            e.Message.Chat.Type)? CommandExecutionState.SUCCESSFUL : CommandExecutionState.UNMET_CONDITIONS;
    }
}