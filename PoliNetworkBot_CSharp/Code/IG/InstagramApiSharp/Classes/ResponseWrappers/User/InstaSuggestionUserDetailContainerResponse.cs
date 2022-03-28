#region

using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaSuggestionUserDetailContainerResponse : InstaDefault
{
    [JsonProperty("items")] public InstaSuggestionItemListResponse Items { get; set; } = new();
}