#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaStoryTrayResponse
{
    [JsonProperty("id")] public long Id { get; set; }

    [JsonProperty("top_live")] public InstaTopLiveResponse TopLive { get; set; } = new();

    [JsonProperty("is_portrait")] public bool IsPortrait { get; set; }

    [JsonProperty("tray")] public List<InstaStoryResponse> Tray { get; set; } = new();
}