using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.DbObject;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;

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
        var x = new List<BackupObjectDescription>
        {
            new("PROCEDURE",
                @"SHOW PROCEDURE STATUS WHERE db = '" + dbConfig.GetDbName() + "' AND type = 'PROCEDURE'; ",
                db.DbBackupDdl.Procedures),
            new("TABLE", "SHOW TABLE STATUS;", db.DbBackupDdl.TablesDdl)
        };
        foreach (var backupObjectDescription in x)
            try
            {
                FillGenericObjects(dbConfig, backupObjectDescription);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
    }

    private static void FillGenericObjects(DbConfigConnection dbConfig, BackupObjectDescription backupObjectDescription)
    {
        try
        {
            var dt = Database.ExecuteSelect(backupObjectDescription.Query, dbConfig);
            if (dt == null)
                return;

            foreach (DataRow dr in dt.Rows)
                try
                {
                    var name = dr["Name"].ToString() ?? "";
                    if (!string.IsNullOrEmpty(name))
                        FillGenericDbObject(dbConfig, name, backupObjectDescription);
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2);
                }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static void FillGenericDbObject(DbConfigConnection dbConfig, string name,
        BackupObjectDescription backupObjectDescription)
    {
        try
        {
            var q = "SHOW CREATE " + backupObjectDescription.ObjectName + " " + dbConfig.GetDbName() + "." + name;
            var dt = Database.ExecuteSelect(q, dbConfig);
            if (dt == null || dt.Rows.Count < 1)
                return;

            backupObjectDescription.Dict[name] = dt.Rows[0];
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static void FillTables(DB_Backup db, DbConfigConnection? dbConfigConnection)
    {
        if (dbConfigConnection == null) return;

        var dbName = dbConfigConnection.GetDbName();
        var q = $"SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA='{dbName}';";
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