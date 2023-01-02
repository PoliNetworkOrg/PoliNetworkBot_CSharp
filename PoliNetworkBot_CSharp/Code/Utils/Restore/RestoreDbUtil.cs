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
using PoliNetworkBot_CSharp.Code.Objects.BackupObj;
using PoliNetworkBot_CSharp.Code.Objects.DbObject;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.Backup;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using PoliNetworkBot_CSharp.Code.Utils.FileUtils;
using PoliNetworkBot_CSharp.Code.Utils.Notify;

namespace PoliNetworkBot_CSharp.Code.Utils.Restore;

public static class RestoreDbUtil
{
    private static async Task<ActionDoneReport?> RestoreDbMethod(string? path)
    {
        if (path == null)
        {
            Console.WriteLine("Restore db failed. 'db.json' is missing");
            return null;
        }

        var s = await File.ReadAllTextAsync(path);
        return RestoreDb_FromFileContent(s);
    }

    private static ActionDoneReport? RestoreDb_FromFileContent(string s)
    {
        if (string.IsNullOrEmpty(s))
            return null;

        var x = JsonConvert.DeserializeObject<DB_Backup?>(s);
        return RestoreDb_FromData(x);
    }

    private static ActionDoneReport? RestoreDb_FromData(DB_Backup? x)
    {
        if (x == null)
            return null;

        DbConfig.InitializeDbConfig();

        x.tables ??= new Dictionary<string, DataTable>();
        var done = x.tables.Sum(y =>
        {
            var xx = TryRestoreTable(y);
            return xx.HowManyDone ?? 0;
        });
        return new ActionDoneReport("RestoreDb_FromData done", null, true, done);
    }

    private static ActionDoneReport TryRestoreTable(KeyValuePair<string, DataTable> y)
    {
        Exception? exception;
        try
        {
            var r = BulkInsert.BulkInsertMySql(y.Value, y.Key, GlobalVariables.DbConfig);
            return new ActionDoneReport("TryRestoreTable " + y.Key + " done", new JObject { ["r"] = r }, r > 0, r);
        }
        catch (Exception ex)
        {
            exception = ex;
            Console.WriteLine(ex);
            Console.WriteLine("Failed import db table named '" + y.Key + "'");
        }

        return new ActionDoneReport(
            "TryRestoreTable " + y.Key + " exception",
            new JObject { ["ex"] = exception.ToString() },
            false,
            0);
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
        var extraValues = new JObject { ["done"] = x?.GetJObject() };
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

        return restoredb_ddl_FromData(x);
    }

    private static ActionDoneReport restoredb_ddl_FromData(DbBackupDdl? x)
    {
        if (x == null)
            return new ActionDoneReport("DbBackupDdl==null", null, false, null);

        var doneProcedures = RestoreProcedures(x.Procedures);
        var doneTables = RestoreTables(x.TablesDdl);

        var sProcedures = doneProcedures.Item1 + "/" + (x.Procedures?.Count ?? 0);
        var sTables = doneTables.Item1 + "/" + (x.TablesDdl?.Count ?? 0);
        var y = "Procedures " + sProcedures + " , tables " + sTables;
        Logger.Logger.WriteLine(y);

        var jObject = new JObject
        {
            ["procedures"] = new JObject
            {
                ["n"] = doneProcedures.Item1,
                ["info"] = doneProcedures.Item2
            },
            ["tables"] = new JObject
            {
                ["n"] = doneTables.Item1,
                ["info"] = doneTables.Item2
            }
        };

        var doneProceduresItem1 = doneProcedures.Item1 + doneTables.Item1;
        return new ActionDoneReport(y, jObject, true, doneProceduresItem1);
    }

    private static Tuple<int, JToken?> RestoreTables(Dictionary<string, DataTable>? xTablesDdl)
    {
        var done = 0;
        var jobjects = new List<JObject>();
        if (xTablesDdl != null)
        {
            var backup = new DB_Backup();
            DbBackup.FillTables(backup, GlobalVariables.DbConfig);
            foreach (var y in xTablesDdl.Select(x => RestoreSingleTable(x, backup)))
            {
                if (y.Done)
                    done++;

                jobjects.Add(new JObject
                {
                    ["done"] = y.Done,
                    ["info"] = y.Extra,
                    ["message"] = y.Message
                });
            }
        }

        var jArray = ActionDoneReport.GetJarrayOfListOfJObjects(jobjects);
        return new Tuple<int, JToken?>(done, jArray);
    }

    private static ActionDoneReport RestoreSingleTable(KeyValuePair<string, DataTable> keyValuePair, DB_Backup backup)
    {
        Exception? exception = null;
        try
        {
            if (backup.TableExists(keyValuePair.Key))
                return new ActionDoneReport("skipped " + keyValuePair.Key, null, false, null);

            var z = TryRestoreTable(keyValuePair);

            return new ActionDoneReport("done return " + keyValuePair.Key, new JObject
            {
                ["name"] = keyValuePair.Key,
                ["ex"] = exception?.ToString(),
                ["done"] = z.Done,
                ["howManyDone"] = z.HowManyDone,
                ["Message"] = z.Message,
                ["Extra"] = z.Extra
            }, z.Done, z.HowManyDone);
        }
        catch (Exception e)
        {
            exception = e;
        }

        return new ActionDoneReport("exception return " + keyValuePair.Key, new JObject
        {
            ["name"] = keyValuePair.Key,
            ["ex"] = exception.ToString()
        }, false, null);
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

            exceptions.Add(new JObject
            {
                ["b"] = b.Item1,
                ["query_create"] = b.Item2,
                ["name"] = procedure.Key,
                ["ex"] = b.Item3?.ToString()
            });
        }


        var jArray = ActionDoneReport.GetJarrayOfListOfJObjects(exceptions);
        return new Tuple<int, JToken?>(done, jArray);
    }

    private static Tuple<bool, string?, Exception?> RestoreProcedure(KeyValuePair<string, DataTable> procedure)
    {
        Exception? ex;
        string? create = null;

        try
        {
            DbConfig.InitializeDbConfig();
            var procedureName = procedure.Value.Rows[0]["Procedure"].ToString();
            create = procedure.Value.Rows[0]["Create Procedure"].ToString();
            create = create?.Replace("`", "");
            create = FixFirstLineProcedure(create, procedureName);

            Database.Execute(create, GlobalVariables.DbConfig);

            return new Tuple<bool, string?, Exception?>(true, create, null);
        }
        catch (Exception ex2)
        {
            ex = ex2;
            Logger.Logger.WriteLine(ex2);
        }

        return new Tuple<bool, string?, Exception?>(false, create, ex);
    }

    private static string? FixFirstLineProcedure(string? create, string? name)
    {
        if (string.IsNullOrEmpty(create) || string.IsNullOrEmpty(name))
            return null;

        var s = create.Split('\n').ToList().Select(x => x.Trim()).ToList();
        s[0] = "CREATE  PROCEDURE " + name + " (";
        var s2 = s.Aggregate((x, y) => x + "\r\n" + y);
        return s2;
    }

    public static async Task<CommandExecutionState> RestoreDb_Full_FromTelegram(MessageEventArgs? arg1,
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
        var x = RestoreDb_full_FromFileContent(text);

        var r = "RestoreDb_Full_FromTelegram [" + x + "]";
        var longs = new List<long?> { arg1?.Message.From?.Id, GroupsConstants.BackupGroup };
        var extraValues = new JObject { ["done"] = ActionDoneReport.GetObject(x) };
        var b = NotifyUtil.SendReportOfExecution(arg1, arg2, longs, r, extraValues);
        Logger.Logger.WriteLine(r + " " + b);

        return CommandExecutionState.SUCCESSFUL;
    }

    private static List<ActionDoneReport?> RestoreDb_full_FromFileContent(string text)
    {
        Exception? exception;
        try
        {
            var x = JsonConvert.DeserializeObject<BackupFull>(text);
            var x1 = RestoreDb_FromData(x?.DbFull);
            var x2 = restoredb_ddl_FromData(x?.DbFullDdl);
            return new List<ActionDoneReport?> { x1, x2 };
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        var jObject = new JObject { ["ex"] = exception.ToString() };
        var actionDoneReport = new ActionDoneReport(
            "RestoreDb_full_FromFileContent exception", jObject, false, null);
        return new List<ActionDoneReport?> { actionDoneReport };
    }
}