#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaRavenMediaActionSummaryResponse
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("timestamp")] public string TimeStamp { get; set; }

        [JsonProperty("count")] public int Count { get; set; }
    }
}