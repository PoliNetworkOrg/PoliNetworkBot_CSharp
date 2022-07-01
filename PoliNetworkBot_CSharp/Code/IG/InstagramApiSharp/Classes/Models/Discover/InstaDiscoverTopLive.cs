#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaDiscoverTopLive
{
    public InstaBroadcastList Broadcasts { get; set; } = new();

    public List<InstaBroadcastPostLive> PostLiveBroadcasts { get; set; } = new();

    public bool MoreAvailable { get; set; }

    public bool AutoLoadMoreEnabled { get; set; }

    public string? NextMaxId { get; set; }
}