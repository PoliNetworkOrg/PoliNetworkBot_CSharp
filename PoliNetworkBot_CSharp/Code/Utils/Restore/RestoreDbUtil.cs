﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.DbObject;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using PoliNetworkBot_CSharp.Code.Utils.FileUtils;
using PoliNetworkBot_CSharp.Code.Utils.Notify;

namespace PoliNetworkBot_CSharp.Code.Utils.Restore;

public static class RestoreDbUtil
{
    private static async Task<int?> RestoreDbMethod(string? path)
    {
        if (path == null)
        {
            Console.WriteLine("Restore db failed. 'db.json' is missing");
            return null;
        }

        var s = await File.ReadAllTextAsync(path);
        return RestoreDb_FromFileContent(s);
    }

    private static int? RestoreDb_FromFileContent(string s)
    {
        if (string.IsNullOrEmpty(s))
            return null;

        var x = JsonConvert.DeserializeObject<DB_Backup?>(s);
        if (x == null)
            return null;

        DbConfig.InitializeDbConfig();

        x.tables ??= new Dictionary<string, DataTable>();
        return x.tables.Sum(y => TryRestoreTable(y) ?? 0);
    }

    private static int? TryRestoreTable(KeyValuePair<string, DataTable> y)
    {
        try
        {
            return BulkInsert.BulkInsertMySql(y.Value, y.Key, GlobalVariables.DbConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("Failed import db table named '" + y.Key + "'");
        }

        return null;
    }

    public static async Task RestoreDb()
    {
        var path = FileUtil.FindFile("db.json");
        var x = await RestoreDbMethod(path);
        Console.WriteLine("PoliNetworkBot_CSharp.Code.Utils.Restore.RestoreDbUtil [RestoreDb] [" + x + "]");
    }

    public static async Task<CommandExecutionState> RestoreDbFromTelegram(MessageEventArgs? arg1,
        TelegramBotAbstract? arg2, string[]? arg3)
    {
        var m = arg1?.Message.ReplyToMessage?.Document;
        if (m == null || arg2 == null)
            return CommandExecutionState.UNMET_CONDITIONS;


        var f = await arg2.DownloadFileAsync(m);
        var stream = f?.Item2;
        if (stream == null) return CommandExecutionState.ERROR_DEFAULT;

        stream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        var x = RestoreDb_FromFileContent(text);

        var r = "RestoreDbFromTelegram [" + x + "]";
        var b = NotifyUtil.SendReportOfExecution(arg1, arg2,
            new List<long?> { arg1?.Message.From?.Id, GroupsConstants.BackupGroup }, r);
        Logger.Logger.WriteLine(r + " " + b);

        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> RestoreDb_Ddl_FromTelegram(MessageEventArgs? arg1,
        TelegramBotAbstract? arg2, string[]? arg3)
    {
        var m = arg1?.Message.ReplyToMessage?.Document;
        if (m == null || arg2 == null)
            return CommandExecutionState.UNMET_CONDITIONS;


        var f = await arg2.DownloadFileAsync(m);
        var stream = f?.Item2;
        if (stream == null) return CommandExecutionState.ERROR_DEFAULT;

        stream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        var x = RestoreDb_ddl_FromFileContent(text);

        var r = "RestoreDb_Ddl_FromTelegram [" + (x ?? "[null]") + "]";
        var b = NotifyUtil.SendReportOfExecution(arg1, arg2,
            new List<long?> { arg1?.Message.From?.Id, GroupsConstants.BackupGroup }, r);
        Logger.Logger.WriteLine(r + " " + b);

        return CommandExecutionState.SUCCESSFUL;
    }

    private static string? RestoreDb_ddl_FromFileContent(string s)
    {
        if (string.IsNullOrEmpty(s))
            return null;

        var x = JsonConvert.DeserializeObject<DbBackupDdl?>(s);
        if (x == null)
            return null;

        var doneProcedures = RestoreProcedures(x.Procedures);

        var sProcedures = doneProcedures + "/" + (x.Procedures?.Count ?? 0);
        var sTables = x.TablesDdl?.Count ?? 0;
        var y = "Procedures " + sProcedures + " , tables " + sTables;
        Logger.Logger.WriteLine(y);
        return y;
    }

    private static int RestoreProcedures(Dictionary<string, DataTable>? xProcedures)
    {
        if (xProcedures == null)
            return 0;

        int done = 0;
        foreach (var procedure in xProcedures)
            try
            {
                var b = RestoreProcedure(procedure);
                if (b)
                    done++;
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteLine(ex);
            }

        return done;
    }

    private static bool RestoreProcedure(KeyValuePair<string, DataTable> procedure)
    {
        DbConfig.InitializeDbConfig();
        var create = procedure.Value.Rows[0]["Create Procedure"].ToString();
        Database.Execute(create, GlobalVariables.DbConfig);
        return true;
    }
}