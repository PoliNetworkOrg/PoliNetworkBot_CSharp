﻿#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters.Users;

internal class InstaFriendshipShortStatusConverter :
    IObjectConverter<InstaFriendshipShortStatus, InstaFriendshipShortStatusResponse>
{
    public InstaFriendshipShortStatusResponse SourceObject { get; init; }

    public InstaFriendshipShortStatus Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");

        var friendships = new InstaFriendshipShortStatus
        {
            Following = SourceObject.Following,
            IncomingRequest = SourceObject.IncomingRequest,
            IsBestie = SourceObject.IsBestie,
            IsPrivate = SourceObject.IsPrivate,
            OutgoingRequest = SourceObject.OutgoingRequest
        };

        return friendships;
    }
}