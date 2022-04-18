﻿#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaUserPresenceContainerResponse
{
    [JsonIgnore] public List<InstaUserPresenceResponse> Items { get; set; } = new();

    [JsonProperty("status")] public string Status { get; set; }
}

public class InstaUserPresenceResponse
{
    [JsonProperty("is_active")] public bool? IsActive { get; set; }

    [JsonProperty("last_activity_at_ms")] public long? LastActivityAtMs { get; set; }

    [JsonIgnore] public long Pk { get; set; }
}