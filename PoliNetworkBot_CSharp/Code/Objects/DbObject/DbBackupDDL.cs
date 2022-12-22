using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class DbBackupDdl
{
    public Dictionary<string, DataTable>? Procedures;
    public Dictionary<string, DataTable>? TablesDdl;
}