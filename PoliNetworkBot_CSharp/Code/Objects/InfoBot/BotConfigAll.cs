﻿using System;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Config;

namespace PoliNetworkBot_CSharp.Code.Objects.InfoBot;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class BotConfigAll
{
    public  BotConfig? BotInfos;
    public  BotConfig? UserBotsInfos;
    public  BotConfig? BotDisguisedAsUserBotInfos;
}