﻿#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaBroadcastSuggestedResponse
    {
        [JsonProperty("broadcasts")] public List<InstaBroadcastResponse> Broadcasts { get; set; }

        [JsonProperty("status")] public string Status { get; set; }
    }
}