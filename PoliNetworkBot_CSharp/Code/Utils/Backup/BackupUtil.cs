#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.Backup;

internal static class BackupUtil
{
    internal static void BackupBeforeReboot()
    {
        MessagesStore.BackupToFile();
        if (CallbackUtils.CallbackUtils.CallBackDataFull != null)
            CallbackUtils.CallbackUtils.CallBackDataFull.BackupToFile();
    }


    public static async Task BackupHandler(long sendTo, TelegramBotAbstract botAbstract, string? username,
        ChatType chatType)
    {
        try
        {
            if (botAbstract.DbConfig != null)
            {
                var jsonDb = DbBackup.GetDB_AsJson(botAbstract.DbConfig);
                SendFile(jsonDb, sendTo, botAbstract, username,
                    chatType, "Backup:", "db.json");
            }
        }
        catch (Exception? ex)
        {
            await NotifyUtil.NotifyOwnerWithLog2(ex, botAbstract, null);
        }
    }

    private static void SendFile(string jsonDb, long sendTo, TelegramBotAbstract botAbstract, string? username,
        ChatType chatType, string? contentMessage, string fileName)
    {
        if (string.IsNullOrEmpty(jsonDb)) return;

        var bytes = Encoding.UTF8.GetBytes(jsonDb);
        var stream = new MemoryStream(bytes);


        var text2 = new Language(new Dictionary<string, string?>
        {
            { "it", contentMessage }
        });

        var peer = new PeerAbstract(sendTo, chatType);


        SendMessage.SendFileAsync(new TelegramFile(stream, fileName,
                null, "application/json"), peer,
            text2, TextAsCaption.BEFORE_FILE,
            botAbstract, username, "it", null, true);
    }


    public static async Task<CommandExecutionState> Backup(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (e?.Message.From == null) return CommandExecutionState.UNMET_CONDITIONS;
        if (sender == null)
            return CommandExecutionState.NOT_TRIGGERED;

        await BackupHandler(e.Message.From.Id, sender, e.Message.From.Username,
            e.Message.Chat.Type);
        return CommandExecutionState.SUCCESSFUL;
    }
}