using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class DbBackupDdl
{
    public Dictionary<string, DataRow> Procedures;
    public Dictionary<string, DataRow> TablesDdl;

    public DbBackupDdl()
    {
        Procedures = new Dictionary<string, DataRow>();
        TablesDdl = new Dictionary<string, DataRow>();
    }
}