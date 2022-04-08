#region

using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaCollectionsResponse : BaseLoadableResponse
{
    [JsonProperty("items")] public List<InstaCollectionItemResponse> Items { get; set; }
}