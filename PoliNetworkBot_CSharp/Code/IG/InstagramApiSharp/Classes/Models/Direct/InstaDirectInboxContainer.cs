#region

using System;
using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Direct;

public class InstaDirectInboxContainer
{
    public int PendingRequestsCount { get; set; }

    public int SeqId { get; set; }

    public InstaDirectInboxSubscription Subscription { get; set; } = new();

    public InstaDirectInbox Inbox { get; set; } = new();

    public List<InstaUserShort> PendingUsers { get; set; } = new();

    public DateTime SnapshotAt { get; set; }
}