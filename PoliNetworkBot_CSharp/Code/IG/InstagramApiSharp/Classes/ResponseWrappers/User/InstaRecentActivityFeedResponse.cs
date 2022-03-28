#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaRecentActivityFeedResponse
{
    [JsonProperty("args")] public InstaRecentActivityStoryItemResponse Args { get; set; }

    [JsonProperty("type")] public int Type { get; set; }

    [JsonProperty("pk")] public string Pk { get; set; }
}