#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.ResponseWrappers.Feed;

public class InstaTopicalExploreFeedResponse : BaseLoadableResponse
{
    [JsonProperty("clusters")] public List<InstaTopicalExploreClusterResponse> Clusters { get; set; } = new();

    [JsonIgnore] public List<InstaMediaItemResponse> Medias { get; set; } = new();

    [JsonIgnore] public InstaChannelResponse Channel { get; set; }

    [JsonIgnore] public List<InstaTVChannelResponse> TvChannels { get; set; } = new();

    [JsonProperty("max_id")] public string? MaxId { get; set; }

    [JsonProperty("has_shopping_channel_content")]
    public bool? HasShoppingChannelContent { get; set; }
}