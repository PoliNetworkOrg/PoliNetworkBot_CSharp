#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaMediaListResponse : BaseLoadableResponse
{
    [JsonProperty("items")] public List<InstaMediaItemResponse> Medias { get; set; } = new();

    public List<InstaStoryResponse> Stories { get; set; } = new();
}