#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaBroadcastNotifyFriendsConverter : IObjectConverter<InstaBroadcastNotifyFriends,
            InstaBroadcastNotifyFriendsResponse>
    {
        public InstaBroadcastNotifyFriendsResponse SourceObject { get; set; }

        public InstaBroadcastNotifyFriends Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var broadcastNotifyFriends = new InstaBroadcastNotifyFriends
            {
                OnlineFriendsCount = SourceObject.OnlineFriendsCount ?? 0,
                Text = SourceObject.Text
            };

            try
            {
                if (SourceObject.Friends?.Count > 0)
                    foreach (var friend in SourceObject.Friends)
                        broadcastNotifyFriends.Friends.Add(ConvertersFabric.Instance
                            .GetUserShortFriendshipFullConverter(friend).Convert());
            }
            catch
            {
            }

            return broadcastNotifyFriends;
        }
    }
}