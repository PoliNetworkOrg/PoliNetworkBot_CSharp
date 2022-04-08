#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaBlockedUsersConverter : IObjectConverter<InstaBlockedUsers, InstaBlockedUsersResponse>
{
    public InstaBlockedUsersResponse SourceObject { get; set; }

    public InstaBlockedUsers Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");

        var blockedUsers = new InstaBlockedUsers
        {
            MaxId = SourceObject.MaxId
        };

        if (SourceObject.BlockedList == null || !SourceObject.BlockedList.Any()) return blockedUsers;
        foreach (var user in SourceObject.BlockedList)
            blockedUsers.BlockedList.Add(ConvertersFabric.GetBlockedUserInfoConverter(user).Convert());

        return blockedUsers;
    }
}