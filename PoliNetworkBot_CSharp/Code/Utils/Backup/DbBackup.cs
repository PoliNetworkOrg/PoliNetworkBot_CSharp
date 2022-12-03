using System.Data;
using System.Linq;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Utils.Backup;

public static class DbBackup
{
    public static string GetDB_AsJson(DbConfigConnection dbConfig)
    {
        try
        {
            DB_Backup db = new();

            FillTables(db, dbConfig);
            FillProcedures(db, dbConfig);
            
            return JsonConvert.SerializeObject(db);
        }
        catch
        {
            // ignored
        }

        return JsonConvert.SerializeObject("ERROR 2");
    }

    private static void FillProcedures(DB_Backup db, DbConfigConnection? dbConfig)
    {
        ; //todo
    }

    private static void FillTables(DB_Backup db, DbConfigConnection? dbConfigConnection)
    {
        const string? q = "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA='polinetwork';";
        if (dbConfigConnection == null) return;

        var r = Database.ExecuteSelect(q, dbConfigConnection);
        if (r == null) return;

        try
        {
            var tableNames = r.Rows.Cast<DataRow>().ToList();

            var c1 = tableNames.Where(row => row is { ItemArray.Length: > 0 } && row.ItemArray[0] != null)
                .Select(row =>
                {
                    var argItem = row.ItemArray[0];
                    return argItem != null ? argItem.ToString() : "";
                });

            foreach (var c3 in c1)
                if (!string.IsNullOrEmpty(c3))
                    db.tableNames.Add(c3);

            foreach (var tableName in
                     db.tableNames.Where(tableName => string.IsNullOrEmpty(tableName) == false))
                try
                {
                    var q2 = "SELECT * FROM " + tableName;
                    var r2 = Database.ExecuteSelect(q2, dbConfigConnection);
                    if (r2 != null) db.tables[tableName] = r2;
                }
                catch
                {
                    // ignored
                }
        }
        catch
        {
            // ignored
        }
    }
}