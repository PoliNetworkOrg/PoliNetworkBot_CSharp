using System;
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

    private static void FillProcedures(DB_Backup db, DbConfigConnection dbConfig)
    {
        try
        {
            const string q = @"SHOW PROCEDURE STATUS WHERE db = 'polinetwork' AND type = 'PROCEDURE'; ";
            var dt = Database.ExecuteSelect(q, dbConfig);
            if (dt == null)
                return;

            foreach (DataRow dr in dt.Rows)
            {
                var name = dr["Name"].ToString() ?? "";
                if (!string.IsNullOrEmpty(name)) FillProcedure(db, dbConfig, name);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static void FillProcedure(DB_Backup db, DbConfigConnection dbConfig, string name)
    {
        try
        {
            var q = "SHOW CREATE PROCEDURE polinetwork." + name;
            var dt = Database.ExecuteSelect(q, dbConfig);
            if (dt == null)
                return;

            if (dt.Rows.Count < 1)
                return;

            var dr = dt.Rows[0];

            var create = dr["Create Procedure"].ToString() ?? "";

            if (!string.IsNullOrEmpty(create)) db.UpdateProcedure(name, create);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
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

            db.AddTables(c1);

            var tables = db.GetTableNames();
            foreach (var tableName in tables)
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