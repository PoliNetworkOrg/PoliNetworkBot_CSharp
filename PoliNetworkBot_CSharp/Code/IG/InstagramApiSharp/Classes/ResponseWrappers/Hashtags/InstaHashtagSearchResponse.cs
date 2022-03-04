#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaHashtagSearchResponse : BaseStatusResponse
    {
        [JsonIgnore] public List<InstaHashtagResponse> Tags { get; set; } = new();

        [JsonProperty("has_more")] public bool? MoreAvailable { get; set; }

        [JsonProperty("rank_token")] public string RankToken { get; set; }
    }
}