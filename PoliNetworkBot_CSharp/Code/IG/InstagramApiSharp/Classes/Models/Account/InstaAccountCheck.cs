#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaAccountCheck : InstaDefaultResponse
{
    [JsonProperty("username")] public string Username { get; set; }

    [JsonProperty("available")] public bool Available { get; set; }

    [JsonProperty("error")] public string Error { get; set; }

    [JsonProperty("error_type")] internal string ErrorType { get; set; }
}