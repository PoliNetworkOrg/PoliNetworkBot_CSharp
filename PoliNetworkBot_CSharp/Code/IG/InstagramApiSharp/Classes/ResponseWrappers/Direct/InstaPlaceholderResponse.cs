#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaPlaceholderResponse
{
    [JsonProperty("is_linked")] public bool IsLinked { get; set; }
    [JsonProperty("message")] public string Message { get; set; }
}