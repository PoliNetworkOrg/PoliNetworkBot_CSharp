using System;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;

[Serializable]
public class MessageThread
{
    public DateTime? DateTime;
    public int? MessageId;
    public long? ChatId;
    public int? IssueNumber;
    public List<MessageThread>? Children;
}