#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
// ReSharper disable once InconsistentNaming
internal class DB_Backup
{
    // ReSharper disable once InconsistentNaming
    public List<string>? tableNames;

    // ReSharper disable once InconsistentNaming
    public Dictionary<string, DataTable>? tables; //indexed by names


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