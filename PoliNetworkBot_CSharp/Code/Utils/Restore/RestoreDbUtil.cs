﻿using System.IO;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Utils.Restore;

public static class RestoreDbUtil
{
    public static async Task RestoreDbMethod(string path)
    {
        var s = await File.ReadAllTextAsync(path);
        if (string.IsNullOrEmpty(s))
            return;
        
        var x = Newtonsoft.Json.JsonConvert.DeserializeObject<DB_Backup?>(s);
        if (x == null)
            return;

        
        DbConfig.InitializeDbConfig();
        foreach (var y in x.tables)
        {
            Utils.Database.BulkInsertMySql(y.Value, y.Key, GlobalVariables.DbConfig );
        }
    }
}