#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaFriendshipStatusConverter :
    IObjectConverter<InstaFriendshipStatus, InstaFriendshipStatusResponse>
{
    public InstaFriendshipStatusResponse SourceObject { get; set; }

    public InstaFriendshipStatus Convert()
    {
        var friendShip = new InstaFriendshipStatus
        {
            Following = SourceObject.Following,
            Blocking = SourceObject.Blocking,
            FollowedBy = SourceObject.FollowedBy,
            OutgoingRequest = SourceObject.OutgoingRequest,
            IsBlockingReel = SourceObject.IsBlockingReel ?? false,
            IncomingRequest = SourceObject.IncomingRequest,
            IsPrivate = SourceObject.IsPrivate
        };
        return friendShip;
    }
}