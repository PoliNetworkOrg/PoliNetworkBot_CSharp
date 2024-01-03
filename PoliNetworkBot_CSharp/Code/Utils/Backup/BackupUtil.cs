#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
<<<<<<< HEAD
using System.Threading;
=======
>>>>>>> dev
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.Backup;

internal static class BackupUtil
{
    private const string applicationJson = "application/json";
    private const string path = "LocalJSONFile.JSON";

    public static List<string> excludedTablesBackupDb = new()
    {
        "LogTable"
    };

    internal static void BackupBeforeReboot()
    {
        MessagesStore.BackupToFile();
        if (CallbackUtils.CallbackUtils.CallBackDataFull != null)
            CallbackUtils.CallbackUtils.CallBackDataFull.BackupToFile();
    }

    public static async Task BackupHandler(List<long?> sendTo, TelegramBotAbstract botAbstract, string? username,
        ChatType chatType)
    {
        if (botAbstract.DbConfig == null) return;


        await BackupDbData(sendTo, botAbstract);
        await BackupDbDdl(sendTo, botAbstract);
    }

    private static async Task BackupDbDdl(List<long?> sendTo, TelegramBotAbstract botAbstract)
    {
        if (botAbstract.DbConfig == null) return;

        try
        {
            var dbFullDdl = DbBackup.Get_DB_DDL_Full(botAbstract.DbConfig);
            const string textToSendBefore = "Backup DB DDL";
            const string dbFullDdlJson = "db_full_ddl.json";

            var serializedText = JsonConvert.SerializeObject(dbFullDdl);
            await SendBackup(
                sendTo,
                botAbstract,
                serializedText,
                textToSendBefore,
                dbFullDdlJson
            );
        }
        catch (Exception? ex)
        {
            await NotifyUtil.NotifyOwnerWithLog2(ex, botAbstract, null);
        }
    }

    private static async Task BackupDbData(List<long?> sendTo, TelegramBotAbstract botAbstract)
    {
        if (botAbstract.DbConfig == null) return;


        try
        {
            DB_Backup db = new();

            DbBackup.FillTableNames(db, botAbstract.DbConfig);

            var dbTableNames = db.tableNames?.Where(x => !excludedTablesBackupDb.Contains(x));
            if (dbTableNames != null)
                foreach (var tableName in dbTableNames)
<<<<<<< HEAD
                {
                    Thread.Sleep(1000);
                    await BackupDbDataSingleTable(tableName, sendTo, botAbstract);
                }
=======
                    await BackupDbDataSingleTable(tableName, sendTo, botAbstract);
>>>>>>> dev
        }
        catch (Exception? ex)
        {
            await NotifyUtil.NotifyOwnerWithLog2(ex, botAbstract, null);
        }
    }


    private static async Task BackupDbDataSingleTable(string tableName, List<long?> sendTo,
        TelegramBotAbstract botAbstract)
    {
        try
        {
            var data = DbBackup.GetDataTable(botAbstract.DbConfig, tableName);
            var textToSendBefore = "Backup DB Data (table: " + tableName + ")";
            var dbFullDataJson = "db_full_data_" + tableName + ".json";

            var serializedText = JsonConvert.SerializeObject(data);
            await SendBackup(
                sendTo,
                botAbstract,
                serializedText,
                textToSendBefore,
                dbFullDataJson
            );
        }
        catch (Exception? ex)
        {
            var jObject = new JObject
            {
                ["tableName"] = tableName
            };
            var eventArgsContainer = new EventArgsContainer { Extra = jObject };
            await NotifyUtil.NotifyOwnerWithLog2(ex, botAbstract, eventArgsContainer);
        }
    }


    private static async Task SendBackup(List<long?> sendTo, TelegramBotAbstract botAbstract,
        string serializedText, string textToSendBefore, string dbFullDataJson)
    {
        try
        {
            await File.WriteAllTextAsync(path, serializedText);
            var stringOrStream = new StringOrStream { StringValue = serializedText };
            LoggerSendFile.SendFiles(sendTo, stringOrStream, botAbstract,
                textToSendBefore, applicationJson, dbFullDataJson);
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

        var fromId = e.Message.From.Id;
        await BackupHandler(new List<long?> { fromId, GroupsConstants.BackupGroup.FullLong() }, sender,
            e.Message.From.Username,
            e.Message.Chat.Type);

        return CommandExecutionState.SUCCESSFUL;
    }
}