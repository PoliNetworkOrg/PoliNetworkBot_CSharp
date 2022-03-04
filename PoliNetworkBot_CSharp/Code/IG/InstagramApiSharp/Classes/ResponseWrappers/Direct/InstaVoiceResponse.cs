#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaVoiceResponse
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("media_type")] public int MediaType { get; set; }

        [JsonProperty("product_type")] public string ProductType { get; set; }

        [JsonProperty("audio")] public InstaAudioResponse Audio { get; set; }

        [JsonProperty("organic_tracking_token")]
        public string OrganicTrackingToken { get; set; }

        [JsonProperty("user")] public InstaVoiceUserResponse User { get; set; }
    }

    public class InstaVoiceUserResponse
    {
        [JsonProperty("pk")] public long Pk { get; set; }

        [JsonProperty("friendship_status")] public InstaFriendshipStatusResponse FriendshipStatus { get; set; }
    }
}