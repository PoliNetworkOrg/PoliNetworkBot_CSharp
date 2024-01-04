using System;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;

[Serializable]
public class MessageThread
{
    public long? ChatId;
    public List<MessageThread>? Children;
    public DateTime? DateTime;
    public int? IssueNumber;
    public int? MessageId;
}