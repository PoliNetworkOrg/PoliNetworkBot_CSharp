#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class FollowedByResponse
{
    [JsonProperty("count")] public int Count { get; set; }
}