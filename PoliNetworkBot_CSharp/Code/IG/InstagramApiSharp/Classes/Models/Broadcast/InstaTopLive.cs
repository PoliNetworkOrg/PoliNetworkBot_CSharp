#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Broadcast;

public class InstaTopLive
{
    public int RankedPosition { get; set; }

    public List<InstaUserShortFriendshipFull> BroadcastOwners { get; set; } = new();
}