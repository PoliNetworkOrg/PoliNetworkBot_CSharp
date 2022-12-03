using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils.FileUtils;

namespace PoliNetworkBot_CSharp.Code.Utils.Restore;

public static class RestoreDbUtil
{
    private static async Task RestoreDbMethod(string? path)
    {
        if (path == null)
        {
            Console.WriteLine("Restore db failed. 'db.json' is missing");
            return;
        }

        var s = await File.ReadAllTextAsync(path);
        if (string.IsNullOrEmpty(s))
            return;

        var x = JsonConvert.DeserializeObject<DB_Backup?>(s);
        if (x == null)
            return;

        DbConfig.InitializeDbConfig();
        foreach (var y in x.tables) TryRestoreTable(y);
    }

    private static void TryRestoreTable(KeyValuePair<string, DataTable> y)
    {
        try
        {
            Database.BulkInsertMySql(y.Value, y.Key, GlobalVariables.DbConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("Failed import db table named '" + y.Key + "'");
        }
    }

    public static async Task RestoreDb()
    {
        var path = FileUtil.FindFile("db.json");
        await RestoreDbMethod(path);
    }
}