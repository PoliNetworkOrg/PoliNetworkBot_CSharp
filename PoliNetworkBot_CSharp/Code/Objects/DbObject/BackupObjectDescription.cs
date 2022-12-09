using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class BackupObjectDescription
{
    public readonly string ObjectName;
    public readonly string Query;
    public Dictionary<string, string> dict;

    public BackupObjectDescription(string objectName, string query, Dictionary<string, string> dict)
    {
        this.ObjectName = objectName;
        this.Query = query;
        this.dict = dict;
    }
}