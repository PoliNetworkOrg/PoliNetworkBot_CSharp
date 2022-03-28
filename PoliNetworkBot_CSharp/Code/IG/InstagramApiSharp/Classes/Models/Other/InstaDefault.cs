#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaDefault
{
    [JsonProperty("status")] public string Status { get; set; }

    [JsonProperty("message")] public string Message { get; set; }
}

public class InstaDefaultResponse : InstaDefault
{
    public bool IsSucceed => Status.ToLower() == "ok";
}