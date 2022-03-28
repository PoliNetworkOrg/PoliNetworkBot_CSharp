#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaBroadcastNotifyFriends
{
    public string Text { get; set; }

    public List<InstaUserShortFriendshipFull> Friends { get; set; } = new();

    public int OnlineFriendsCount { get; set; }
}