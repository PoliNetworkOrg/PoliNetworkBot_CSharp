﻿#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaOembedUrlResponse
{
    [JsonProperty("media_id")] //media_id is enough.
    public string MediaId { get; set; }
}