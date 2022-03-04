﻿#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaStoryFriendshipStatusConverter : IObjectConverter<InstaStoryFriendshipStatus,
            InstaStoryFriendshipStatusResponse>
    {
        public InstaStoryFriendshipStatusResponse SourceObject { get; set; }

        public InstaStoryFriendshipStatus Convert()
        {
            var storyFriendshipStatus = new InstaStoryFriendshipStatus
            {
                Following = SourceObject.Following,
                Blocking = SourceObject.Blocking ?? false,
                FollowedBy = SourceObject.FollowedBy,
                OutgoingRequest = SourceObject.OutgoingRequest ?? false,
                IsBestie = SourceObject.IsBestie ?? false,
                Muting = SourceObject.Muting ?? false,
                IncomingRequest = SourceObject.IncomingRequest ?? false,
                IsBlockingReel = SourceObject.IsBlockingReel ?? false,
                IsMutingReel = SourceObject.IsMutingReel ?? false,
                IsPrivate = SourceObject.IsPrivate
            };
            return storyFriendshipStatus;
        }
    }
}