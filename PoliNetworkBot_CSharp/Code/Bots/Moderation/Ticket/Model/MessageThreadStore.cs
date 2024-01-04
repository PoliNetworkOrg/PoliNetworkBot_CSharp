using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MessageThreadStore
{
    public Dictionary<DateTime, List<MessageThread>>? Dict;
}