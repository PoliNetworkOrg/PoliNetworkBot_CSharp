#region

using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaPermalinkResponse : BaseStatusResponse
    {
        [JsonProperty("permalink")] public string Permalink { get; set; }
    }
}