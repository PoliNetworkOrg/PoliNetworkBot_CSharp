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
    public Dictionary<string, string>? Procedures;

    // ReSharper disable once InconsistentNaming
    public Dictionary<string, DataTable> tables; //indexed by names

    public DB_Backup()
    {
        tableNames = new List<string>();
        tables = new Dictionary<string, DataTable>();
        Procedures = new Dictionary<string, string>();
    }

    public void UpdateProcedure(string name, string create)
    {
        Procedures ??= new Dictionary<string, string>();
        Procedures[name] = create;
    }

    public void AddTables(IEnumerable<string?> c1)
    {
        this.tableNames ??= new List<string>();
        foreach (var c3 in c1)
            if (!string.IsNullOrEmpty(c3))
                this.tableNames.Add(c3);
    }

    public IEnumerable<string> GetTableNames()
    {
        this.tableNames ??= new List<string>();
        return this.tableNames.Where(tableName => string.IsNullOrEmpty(tableName) == false);
    }
}