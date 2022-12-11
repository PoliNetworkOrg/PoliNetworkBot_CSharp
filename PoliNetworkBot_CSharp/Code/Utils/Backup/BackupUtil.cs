#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
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
                const string applicationJson = "application/json";
                var jsonDb = DbBackup.GetDB_AsJson(botAbstract.DbConfig);
                var sendToList = new List<long?>(){sendTo};
                Logger.LoggerSendFile.SendFiles(sendToList, jsonDb, botAbstract, "Backup:", applicationJson);
            }
        }
        catch (Exception? ex)
        {
            await NotifyUtil.NotifyOwnerWithLog2(ex, botAbstract, null);
        }
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