using System;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class ExtraInfo
{
    public string? StackTrace;
    public string? GenericInfo;
}