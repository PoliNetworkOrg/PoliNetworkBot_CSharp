using System;
using System.Collections.Generic;
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

            FillDdl(db, dbConfig);
            
    

            return JsonConvert.SerializeObject(db);
        }
        catch
        {
            // ignored
        }

        return JsonConvert.SerializeObject("ERROR 2");
    }

    private static void FillDdl(DB_Backup db, DbConfigConnection dbConfig)
    {
        var x = new List<Tuple<string, string>>
        {
            new("PROCEDURE", @"SHOW PROCEDURE STATUS WHERE db = 'polinetwork' AND type = 'PROCEDURE'; "),
            new("TABLE", "SHOW TABLE STATUS;")
        };
        foreach (var x2 in x)
        {
            FillProcedures(db, dbConfig, x2.Item1, x2.Item2);
        }
    }

    private static void FillProcedures(DB_Backup db, DbConfigConnection dbConfig, string procedure, string q)
    {
        try
        {
            var dt = Database.ExecuteSelect(q, dbConfig);
            if (dt == null)
                return;

            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    var name = dr["Name"].ToString() ?? "";
                    if (!string.IsNullOrEmpty(name))
                        FillGenericDbObject(db, dbConfig, name, db.Procedures, procedure);
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static void FillGenericDbObject(DB_Backup db, DbConfigConnection dbConfig, string name,
        IDictionary<string, string> dbProcedures, string procedure)
    {
        try
        {
            var q = "SHOW CREATE "+procedure+" "+dbConfig.GetDbName()+"." + name;
            var dt = Database.ExecuteSelect(q, dbConfig);
            if (dt == null)
                return;

            if (dt.Rows.Count < 1)
                return;

            var dr = dt.Rows[0];

            var create = dr.ItemArray[1]?.ToString() ?? "";

            if (!string.IsNullOrEmpty(create))
            {
                dbProcedures[name] = create;
            }
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