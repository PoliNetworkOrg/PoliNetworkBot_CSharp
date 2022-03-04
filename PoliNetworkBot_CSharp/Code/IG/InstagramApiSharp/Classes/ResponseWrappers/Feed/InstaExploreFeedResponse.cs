#region

using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaExploreFeedResponse : BaseLoadableResponse
    {
        [JsonIgnore] public InstaExploreItemsResponse Items { get; set; } = new();

        [JsonProperty("max_id")] public string MaxId { get; set; }
    }
}