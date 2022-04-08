#region

using Newtonsoft.Json;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaTVSearchResponse
{
    [JsonProperty("results")] public List<InstaTVSearchResultResponse> Results { get; set; }

    [JsonProperty("num_results")] public int? NumResults { get; set; }

    [JsonProperty("rank_token")] public string RankToken { get; set; }

    [JsonProperty("status")] internal string Status { get; set; }
}

public class InstaTVSearchResultResponse
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("user")] public InstaUserShortFriendshipResponse User { get; set; }

    [JsonProperty("channel")] public InstaTVChannelResponse Channel { get; set; }
}