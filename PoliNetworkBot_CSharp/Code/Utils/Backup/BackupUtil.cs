#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.BackupObj;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using SampleNuGet.Objects;
using SampleNuGet.Utils;
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


    public static async Task BackupHandler(List<long?> sendTo, TelegramBotAbstract botAbstract, string? username,
        ChatType chatType, int? messageThreadId)
    {
        try
        {
            if (botAbstract.DbConfig != null)
            {
                const string applicationJson = "application/json";
                var dbFull = DbBackup.GetDb_Full(botAbstract.DbConfig);
                var dbFullDdl = DbBackup.Get_DB_DDL_Full(botAbstract.DbConfig);
                var backupFull = new BackupFull(dbFull, dbFullDdl);
                const string path = "LocalJSONFile.JSON";
                await using (TextWriter writer = File.CreateText(path))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, backupFull);
                }

                var serializedText = await File.ReadAllTextAsync(path);
                LoggerSendFile.SendFiles(sendTo, serializedText, botAbstract,
                    "Backup DB", applicationJson, "db_full.json", messageThreadId);
            }
        }
        catch (Exception? ex)
        {
            await NotifyUtil.NotifyOwnerWithLog2(ex, botAbstract, null);
        }
    }


    public static void Backup(ActionFuncGenericParams actionFuncGenericParams)
    {
        var e = actionFuncGenericParams.MessageEventArgs;
        var sender = actionFuncGenericParams.TelegramBotAbstract;
        if (e?.Message.From == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        if (sender == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.NOT_TRIGGERED;
            return;
        }

        var eMessage = e.Message;
        var eMessageFrom = eMessage.From;
        var fromId = eMessageFrom.Id;
        var backupHandler = BackupHandler(new List<long?> { fromId, GroupsConstants.BackupGroup }, sender,
            eMessageFrom.Username,
            eMessage.Chat.Type, eMessage.MessageThreadId);
        backupHandler.Wait();

        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }
}