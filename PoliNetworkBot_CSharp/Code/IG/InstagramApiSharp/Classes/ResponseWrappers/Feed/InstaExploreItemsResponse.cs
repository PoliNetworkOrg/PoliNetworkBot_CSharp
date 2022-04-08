#region

using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaExploreItemsResponse : BaseLoadableResponse
{
    [JsonIgnore] public InstaStoryTrayResponse StoryTray { get; set; } = new();

    [JsonIgnore] public List<InstaMediaItemResponse> Medias { get; set; } = new();

    [JsonIgnore] public InstaChannelResponse Channel { get; set; }
}