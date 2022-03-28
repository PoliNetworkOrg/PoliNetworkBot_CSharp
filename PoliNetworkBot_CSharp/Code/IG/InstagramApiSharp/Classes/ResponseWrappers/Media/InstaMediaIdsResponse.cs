#region

using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaMediaIdsResponse : InstaDefault
{
    [JsonProperty("media_ids")] public InstaMediaIdList MediaIds = new();
}