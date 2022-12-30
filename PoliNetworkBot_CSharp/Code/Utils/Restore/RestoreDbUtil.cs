using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
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
        var longs = new List<long?> { arg1?.Message.From?.Id, GroupsConstants.BackupGroup };
        var extraValues = new JObject { ["done"] = x };
        var b = NotifyUtil.SendReportOfExecution(arg1, arg2, longs, r, extraValues);
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

        var r = "RestoreDb_Ddl_FromTelegram [" + (x?.Message ?? "[null]") + "]";
        var longs = new List<long?> { arg1?.Message.From?.Id, GroupsConstants.BackupGroup };
        var b = NotifyUtil.SendReportOfExecution(arg1, arg2, longs, r, x?.Extra);
        Logger.Logger.WriteLine(r + " " + b);

        return CommandExecutionState.SUCCESSFUL;
    }

    private static ActionDoneReport? RestoreDb_ddl_FromFileContent(string s)
    {
        if (string.IsNullOrEmpty(s))
            return null;

        var x = JsonConvert.DeserializeObject<DbBackupDdl?>(s);
        if (x == null)
            return null;

        var doneProcedures = RestoreProcedures(x.Procedures);

        var sProcedures = doneProcedures.Item1 + "/" + (x.Procedures?.Count ?? 0);
        var sTables = x.TablesDdl?.Count ?? 0;
        var y = "Procedures " + sProcedures + " , tables " + sTables;
        Logger.Logger.WriteLine(y);

        var jObject = new JObject
        {
            ["procedures"] = new JObject
            {
                ["n"] = doneProcedures.Item1,
                ["info"] = doneProcedures.Item2
            }
        };
        return new ActionDoneReport(y, jObject);
    }

    private static Tuple<int, JToken?> RestoreProcedures(Dictionary<string, DataTable>? xProcedures)
    {
        if (xProcedures == null)
            return new Tuple<int, JToken?>(0, null);

        var done = 0;
        var exceptions = new List<JObject>();
        foreach (var procedure in xProcedures)
        {
            var b = RestoreProcedure(procedure);
            if (b.Item1)
                done++;

            exceptions.Add(new JObject()
            {
                ["b"] = b.Item1,
                ["qc"] = b.Item2,
                ["qcd"] = b.Item3,
                ["name"] = procedure.Key,
                ["ex"] = b.Item4?.ToString()
            });
        }


        var jArray = ActionDoneReport.GetJarrayOfListOfJObjects(exceptions);
        return new Tuple<int, JToken?>(done, jArray);
    }

    private static Tuple<bool, string?, string?, Exception?> RestoreProcedure(KeyValuePair<string, DataTable> procedure)
    {
        Exception? ex;
        string? create = null;
        string? c2 = null;
        try
        {
            DbConfig.InitializeDbConfig();
            create = procedure.Value.Rows[0]["Create Procedure"].ToString();
            c2 = "DELIMITER //\n" + create + "//\nDELIMITER ;";
            Database.Execute(c2, GlobalVariables.DbConfig);
            return new Tuple<bool, string?, string?, Exception?>(true, create, c2, null);
        }
        catch (Exception ex2)
        {
            ex = ex2;
            Logger.Logger.WriteLine(ex2);
        }

        return new Tuple<bool, string?, string?, Exception?>(false, create, c2, ex);
    }
}