#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaFelixShareResponse
    {
        [JsonProperty("video")] public InstaMediaItemResponse Video { get; set; }
    }
}