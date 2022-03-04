#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaTagFeedResponse : InstaMediaListResponse
    {
        [JsonProperty("ranked_items")] public List<InstaMediaItemResponse> RankedItems { get; set; } = new();
    }
}