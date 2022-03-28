#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaPayloadResponse
{
    [JsonProperty("client_context")] public string ClientContext { get; set; }

    [JsonProperty("message")] public string Message { get; set; }
}