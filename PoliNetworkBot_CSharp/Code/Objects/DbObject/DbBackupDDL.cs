using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class DbBackupDdl
{
    public Dictionary<string, string> Procedures;
    public Dictionary<string, string> TablesDdl;

    public DbBackupDdl()
    {
        Procedures = new Dictionary<string, string>();
        TablesDdl = new Dictionary<string, string>();
    }
}