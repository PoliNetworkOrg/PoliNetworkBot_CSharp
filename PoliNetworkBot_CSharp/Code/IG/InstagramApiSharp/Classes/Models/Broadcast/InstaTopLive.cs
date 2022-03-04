#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaTopLive
    {
        public int RankedPosition { get; set; }

        public List<InstaUserShortFriendshipFull> BroadcastOwners { get; set; } = new();
    }
}