#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaFriendshipFullStatusConverter : IObjectConverter<InstaFriendshipFullStatus,
            InstaFriendshipFullStatusResponse>
    {
        public InstaFriendshipFullStatusResponse SourceObject { get; set; }

        public InstaFriendshipFullStatus Convert()
        {
            var friendShip = new InstaFriendshipFullStatus
            {
                Following = SourceObject.Following ?? false,
                Blocking = SourceObject.Blocking ?? false,
                FollowedBy = SourceObject.FollowedBy ?? false,
                OutgoingRequest = SourceObject.OutgoingRequest ?? false,
                IsBestie = SourceObject.IsBestie ?? false,
                Muting = SourceObject.Muting ?? false,
                IncomingRequest = SourceObject.IncomingRequest ?? false,
                IsPrivate = SourceObject.IsPrivate ?? false
            };
            return friendShip;
        }
    }
}