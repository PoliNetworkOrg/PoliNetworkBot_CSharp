#region

using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
// ReSharper disable once InconsistentNaming
internal class DB_Backup
{
    // ReSharper disable once InconsistentNaming
    public List<string> tableNames;

    // ReSharper disable once InconsistentNaming
    public Dictionary<string, DataTable> tables; //indexed by names

    public DB_Backup()
    {
        tableNames = new List<string>();
        tables = new Dictionary<string, DataTable>();
    }
}