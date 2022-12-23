using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class BackupObjectDescription
{
    public readonly string ObjectName;
    public readonly string Query;
    public Dictionary<string, DataTable>? Dict;

    public BackupObjectDescription(string objectName, string query, Dictionary<string, DataTable>? dict)
    {
        ObjectName = objectName;
        Query = query;
        Dict = dict;
    }
}