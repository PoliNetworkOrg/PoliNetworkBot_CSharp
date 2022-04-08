#region

using Newtonsoft.Json;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaBroadcastAddToPostLiveContainerResponse
{
    [JsonProperty("post_live_items")] public List<InstaBroadcastAddToPostLiveResponse> PostLiveItems { get; set; }
}

public class InstaBroadcastAddToPostLiveResponse
{
    [JsonProperty("pk")] public string Pk { get; set; }

    [JsonProperty("user")] public InstaUserShortFriendshipFullResponse User { get; set; }

    [JsonProperty("broadcasts")] public List<InstaBroadcastResponse> Broadcasts { get; set; } = new();

    [JsonProperty("last_seen_broadcast_ts")]
    public double? LastSeenBroadcastTs { get; set; }

    [JsonProperty("can_reply")] public bool CanReply { get; set; }

    [JsonProperty("status")] public string Status { get; set; }

    [JsonProperty("dash_manifest")] public string DashManifest { get; set; }
}