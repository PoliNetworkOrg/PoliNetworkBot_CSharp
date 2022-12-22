using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using PoliNetworkBot_CSharp.Code.Utils.FileUtils;

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
}