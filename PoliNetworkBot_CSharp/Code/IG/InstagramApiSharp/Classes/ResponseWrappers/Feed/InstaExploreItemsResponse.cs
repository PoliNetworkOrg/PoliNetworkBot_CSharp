#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaExploreItemsResponse : BaseLoadableResponse
{
    [JsonIgnore] public InstaStoryTrayResponse StoryTray { get; set; } = new();

    [JsonIgnore] public List<InstaMediaItemResponse> Medias { get; set; } = new();

    [JsonIgnore] public InstaChannelResponse Channel { get; set; }
}