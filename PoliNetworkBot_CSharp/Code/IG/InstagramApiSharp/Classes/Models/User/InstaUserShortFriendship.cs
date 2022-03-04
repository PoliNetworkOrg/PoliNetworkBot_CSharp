#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaUserShortFriendshipList : List<InstaUserShortFriendship>
    {
    }

    public class InstaUserShortFriendship : InstaUserShort
    {
        public InstaFriendshipShortStatus FriendshipStatus { get; set; }
    }
}