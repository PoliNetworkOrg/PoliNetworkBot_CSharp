#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaUserShortFriendshipResponse : InstaUserShortResponse
    {
        [JsonProperty("friendship_status")] public InstaFriendshipShortStatusResponse FriendshipStatus { get; set; }
    }
}