#region

using Newtonsoft.Json;
using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class Config
{
    public string Password;
    public string RootDir;
}