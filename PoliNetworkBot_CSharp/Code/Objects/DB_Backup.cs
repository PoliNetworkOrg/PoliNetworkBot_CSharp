#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects.DbObject;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
// ReSharper disable once InconsistentNaming
internal class DB_Backup
{
    public DbBackupDdl DbBackupDdl;

    // ReSharper disable once InconsistentNaming
    public List<string>? tableNames;

    // ReSharper disable once InconsistentNaming
    public Dictionary<string, DataTable> tables; //indexed by names


    public DB_Backup()
    {
        tableNames = new List<string>();
        tables = new Dictionary<string, DataTable>();
        DbBackupDdl = new DbBackupDdl();
    }


    public void AddTables(IEnumerable<string?> c1)
    {
        tableNames ??= new List<string>();
        foreach (var c3 in c1)
            if (!string.IsNullOrEmpty(c3))
                tableNames.Add(c3);
    }

    public IEnumerable<string> GetTableNames()
    {
        tableNames ??= new List<string>();
        return tableNames.Where(tableName => string.IsNullOrEmpty(tableName) == false);
    }
}