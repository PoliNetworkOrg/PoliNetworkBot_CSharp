#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaUserShortFriendshipFullResponse : InstaUserShortResponse
    {
        [JsonProperty("friendship_status")] public InstaFriendshipFullStatusResponse FriendshipStatus { get; set; }
    }
}