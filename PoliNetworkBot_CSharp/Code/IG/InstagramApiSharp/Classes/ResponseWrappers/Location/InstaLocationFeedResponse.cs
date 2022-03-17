#region

using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaLocationFeedResponse : BaseLoadableResponse
    {
        [JsonProperty("ranked_items")] public List<InstaMediaItemResponse> RankedItems { get; set; } = new();

        [JsonProperty("items")] public List<InstaMediaItemResponse> Items { get; set; } = new();

        [JsonProperty("story")] public InstaStoryResponse Story { get; set; }

        [JsonProperty("media_count")] public long MediaCount { get; set; }

        [JsonProperty("location")] public InstaLocationResponse Location { get; set; }
    }
}