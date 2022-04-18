#region

using System.Collections.Generic;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.ResponseWrappers.Media;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaTagFeedResponse : InstaMediaListResponse
{
    [JsonProperty("ranked_items")] public List<InstaMediaItemResponse> RankedItems { get; set; } = new();
}