#region

using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaTopicalExploreFeedResponse : BaseLoadableResponse
{
    [JsonProperty("clusters")] public List<InstaTopicalExploreClusterResponse> Clusters { get; set; } = new();

    [JsonIgnore] public List<InstaMediaItemResponse> Medias { get; set; } = new();

    [JsonIgnore] public InstaChannelResponse Channel { get; set; }

    [JsonIgnore] public List<InstaTVChannelResponse> TVChannels { get; set; } = new();

    [JsonProperty("max_id")] public string MaxId { get; set; }

    [JsonProperty("has_shopping_channel_content")]
    public bool? HasShoppingChannelContent { get; set; }
}