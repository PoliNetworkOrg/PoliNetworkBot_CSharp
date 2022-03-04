﻿#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaUserTagListResponse
    {
        [JsonProperty("in")] public List<InstaUserTagResponse> In { get; set; } = new();
    }
}