using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class ExtraInfo
{
    public string? StackTrace;
    public string? GenericInfo;

    public JToken GetJToken()
    {
        var result = new JObject
        {
            ["StackTrace"] = StackTrace,
            ["GenericInfo"] = GenericInfo
        };
        return result;
    }
}