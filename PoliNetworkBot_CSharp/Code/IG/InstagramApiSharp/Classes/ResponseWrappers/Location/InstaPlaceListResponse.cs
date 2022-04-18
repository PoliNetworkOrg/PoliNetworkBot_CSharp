#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaPlaceListResponse
{
    [JsonIgnore] public readonly List<long> ExcludeList = new();
    [JsonProperty("items")] public List<InstaPlaceResponse> Items { get; set; }

    [JsonProperty("has_more")] public bool? HasMore { get; set; }

    [JsonProperty("rank_token")] public string RankToken { get; set; }

    [JsonProperty("status")] internal string Status { get; set; }

    [JsonProperty("message")] internal string Message { get; set; }
}