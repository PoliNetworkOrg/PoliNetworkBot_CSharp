using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.DbObject;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;

namespace PoliNetworkBot_CSharp.Code.Utils.Backup;

public static class DbBackup
{
    public static DbBackupDdl? Get_DB_DDL_Full(DbConfigConnection dbConfig)
    {
        try
        {
            var db = FillDdl(dbConfig);

            return db;
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static DbBackupDdl FillDdl(DbConfigConnection dbConfig)
    {
        var dbBackupDdl = new DbBackupDdl();
        dbBackupDdl.Procedures ??= new Dictionary<string, DataTable>();
        dbBackupDdl.TablesDdl ??= new Dictionary<string, DataTable>();

        var x = new List<BackupObjectDescription>
        {
            new("PROCEDURE",
                @"SHOW PROCEDURE STATUS WHERE db = '" + dbConfig.GetDbName() + "' AND type = 'PROCEDURE'; ",
                dbBackupDdl.Procedures),
            new("TABLE", "SHOW TABLE STATUS;", dbBackupDdl.TablesDdl)
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

        return dbBackupDdl;
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
        backupObjectDescription.Dict ??= new Dictionary<string, DataTable>();

        try
        {
            var q = "SHOW CREATE " + backupObjectDescription.ObjectName + " " + dbConfig.GetDbName() + "." + name;
            var dt = Database.ExecuteSelect(q, dbConfig);
            if (dt == null || dt.Rows.Count < 1)
                return;

            backupObjectDescription.Dict[name] = dt;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static void FillTableNames(DB_Backup db, DbConfigConnection? dbConfigConnection)
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
        }
        catch
        {
            // ignored
        }

        db.tables ??= new Dictionary<string, DataTable>();
    }

    public static void FillTablesData(DB_Backup db, DbConfigConnection? dbConfigConnection)
    {
        if (dbConfigConnection == null) return;


        if (db.tableNames == null || db.tableNames.Count == 0) FillTableNames(db, dbConfigConnection);


        try
        {
            var tables = db.GetTableNames();
            db.tables ??= new Dictionary<string, DataTable>();
            foreach (var tableName in tables) FillSingleDataTable(db, dbConfigConnection, tableName);
        }
        catch
        {
            // ignored
        }
    }

    private static void FillSingleDataTable(DB_Backup db, DbConfigConnection dbConfigConnection, string tableName)
    {
        try
        {
            var r2 = GetDataTable(dbConfigConnection, tableName);
            if (r2 == null) return;
            if (db.tables != null)
                db.tables[tableName] = r2;
        }
        catch
        {
            // ignored
        }
    }

    public static DataTable? GetDataTable(DbConfigConnection? dbConfigConnection, string tableName)
    {
        if (dbConfigConnection == null)
            return null;

        var q2 = "SELECT * FROM " + tableName;
        var r2 = Database.ExecuteSelect(q2, dbConfigConnection);
        return r2;
    }
}