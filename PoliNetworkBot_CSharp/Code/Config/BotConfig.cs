using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using System;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Config
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    internal class BotConfig
    {
        public List<BotInfoAbstract> bots = null;
    }
}