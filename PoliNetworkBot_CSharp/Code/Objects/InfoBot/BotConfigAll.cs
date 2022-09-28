#region

using System;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Config;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.InfoBot;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class BotConfigAll
{
    public BotConfig? BotDisguisedAsUserBotInfos;
    public BotConfig? BotInfos;
    public BotConfig? UserBotsInfos;
}