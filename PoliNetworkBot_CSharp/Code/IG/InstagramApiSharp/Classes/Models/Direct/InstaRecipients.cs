#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaRecipients
{
    public List<InstaRankedRecipientThread> Threads { get; set; } = new();

    public List<InstaUserShort> Users { get; set; } = new();

    public long ExpiresIn { get; set; }

    public bool Filtered { get; set; }

    public string RankToken { get; set; }

    public string RequestId { get; set; }
}