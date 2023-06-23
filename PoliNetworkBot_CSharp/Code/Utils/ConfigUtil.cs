#region

using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Objects.TmpResults;
using PoliNetworkBot_CSharp.Code.Utils.Main;
using SampleNuGet.Enums;
using SampleNuGet.Objects;
using SampleNuGet.Objects.TelegramMedia;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class ConfigUtil
{
    public static bool GetConfig(long? fromId, string? fromUsername, TelegramBotAbstract? sender,
        string? fromLanguageCode, ChatType? chatType, int? messageThreadId)
    {
        var objectToSend = new ObjectToSend { FileName = "config.json", Value = ProgramUtil.BotConfigAll };
        return GetFile(objectToSend, fromId, fromUsername, sender, fromLanguageCode, chatType, messageThreadId);
    }


    private static bool GetFile(ObjectToSend configJson, long? fromId, string? fromUsername,
        TelegramBotAbstract? sender, string? fromLanguageCode, ChatType? chatType, int? messageThreadId)
    {
        var json = JsonConvert.SerializeObject(configJson.Value);
        var file = TelegramFile.FromString(json, configJson.FileName ?? "file.json", new L(), TextAsCaption.AS_CAPTION);
        chatType ??= ChatType.Private;

        var peer = new PeerAbstract(fromId, chatType.Value);
        return SendMessage.SendFileAsync(file, peer, sender, fromUsername, messageThreadId,
            fromLanguageCode, null, true);
    }

    public static bool GetDbConfig(MessageEventArgs? messageEventArgs, TelegramBotAbstract? telegramBotAbstract)
    {
        var dbConfigConnection = telegramBotAbstract?.DbConfig;
        if (dbConfigConnection == null) return false;
        var objectToSend = new ObjectToSend { Value = dbConfigConnection.GetDbConfig(), FileName = "dbconfig.json" };
        var message1 = messageEventArgs?.Message;
        var message1From = message1?.From;
        return message1From != null && GetFile(objectToSend, message1From.Id,
            message1From.Username,
            telegramBotAbstract, message1From.LanguageCode, ChatType.Private, message1?.MessageThreadId);
    }
}