using System;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Utils

{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class Config
    {
        public string Token;
        public string Password;
        public string RootDir;
    }
}