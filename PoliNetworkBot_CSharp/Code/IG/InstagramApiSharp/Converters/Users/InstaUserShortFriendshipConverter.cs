#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaUserShortFriendshipConverter : IObjectConverter<InstaUserShortFriendship, InstaUserShortFriendshipResponse>
    {
        public InstaUserShortFriendshipResponse SourceObject { get; set; }

        public InstaUserShortFriendship Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var user = new InstaUserShortFriendship
            {
                Pk = SourceObject.Pk,
                UserName = SourceObject.UserName,
                FullName = SourceObject.FullName,
                IsPrivate = SourceObject.IsPrivate,
                ProfilePicture = SourceObject.ProfilePicture,
                ProfilePictureId = SourceObject.ProfilePictureId,
                IsVerified = SourceObject.IsVerified,
                ProfilePicUrl = SourceObject.ProfilePicture
            };
            if (SourceObject.FriendshipStatus == null) return user;
            var item = SourceObject.FriendshipStatus;
            var friend = new InstaFriendshipShortStatus
            {
                Following = item.Following,
                IncomingRequest = item.IncomingRequest,
                IsBestie = item.IsBestie,
                IsPrivate = item.IsPrivate,
                OutgoingRequest = item.OutgoingRequest,
                Pk = 0
            };
            user.FriendshipStatus = friend;

            return user;
        }
    }
}