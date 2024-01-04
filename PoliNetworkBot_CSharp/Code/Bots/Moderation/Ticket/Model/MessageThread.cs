using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MessageThread
{
    public long? ChatId;
    public List<MessageThread>? Children;
    public GithubInfo? GithubInfo;
    public int? IssueNumber;
    public int? MessageId;
}