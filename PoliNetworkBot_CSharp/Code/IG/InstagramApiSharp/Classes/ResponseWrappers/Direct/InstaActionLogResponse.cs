#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaActionLogResponse
    {
        [JsonProperty("description")] public string Description { get; set; }
    }
}