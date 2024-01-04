#region

using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Objects.TmpResults;
using PoliNetworkBot_CSharp.Code.Utils.Main;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class ConfigUtil
{
    public static bool GetConfig(long? fromId, string? fromUsername, TelegramBotAbstract? sender,
        string? fromLanguageCode, ChatType? chatType)
    {
        var objectToSend = new ObjectToSend { FileName = "config.json", Value = ProgramUtil.BotConfigAll };
        return GetFile(objectToSend, fromId, fromUsername, sender, fromLanguageCode, chatType);
    }


    private static bool GetFile(ObjectToSend configJson, long? fromId, string? fromUsername,
        TelegramBotAbstract? sender, string? fromLanguageCode, ChatType? chatType)
    {
        var json = JsonConvert.SerializeObject(configJson.Value);
        var file = TelegramFile.FromString(json, configJson.FileName ?? "file.json", new L(), TextAsCaption.AS_CAPTION);
        chatType ??= ChatType.Private;

        var peer = new PeerAbstract(fromId, chatType.Value);
        return SendMessage.SendFileAsync(file, peer, sender, fromUsername,
            fromLanguageCode, null, true);
    }

    public static bool GetDbConfig(MessageEventArgs? messageEventArgs, TelegramBotAbstract? telegramBotAbstract)
    {
        var dbConfigConnection = telegramBotAbstract?.DbConfig;
        if (dbConfigConnection == null) return false;
        var objectToSend = new ObjectToSend { Value = dbConfigConnection.GetDbConfig(), FileName = "dbconfig.json" };
        return messageEventArgs?.Message.From != null && GetFile(objectToSend, messageEventArgs.Message.From.Id,
            messageEventArgs.Message.From.Username,
            telegramBotAbstract, messageEventArgs.Message.From.LanguageCode, ChatType.Private);
    }
}