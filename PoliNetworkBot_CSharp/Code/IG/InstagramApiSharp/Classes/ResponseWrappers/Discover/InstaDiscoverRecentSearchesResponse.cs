﻿#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaDiscoverRecentSearchesResponse
    {
        [JsonProperty("recent")] public List<InstaDiscoverSearchesResponse> Recent { get; set; }

        [JsonProperty("status")] public string Status { get; set; }
    }
}