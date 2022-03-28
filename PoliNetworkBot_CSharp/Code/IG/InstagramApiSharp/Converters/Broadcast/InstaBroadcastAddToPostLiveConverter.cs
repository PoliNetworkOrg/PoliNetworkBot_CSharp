#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaBroadcastAddToPostLiveConverter : IObjectConverter<InstaBroadcastAddToPostLive,
            InstaBroadcastAddToPostLiveResponse>
    {
        public InstaBroadcastAddToPostLiveResponse SourceObject { get; set; }

        public InstaBroadcastAddToPostLive Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var postlive = new InstaBroadcastAddToPostLive
            {
                CanReply = SourceObject.CanReply,
                LastSeenBroadcastTs = SourceObject.LastSeenBroadcastTs ?? 0,
                Pk = SourceObject.Pk
            };

            if (SourceObject.User != null)
                postlive.User = ConvertersFabric.GetUserShortFriendshipFullConverter(SourceObject.User)
                    .Convert();

            if (SourceObject.Broadcasts?.Count > 0)
                postlive.Broadcasts = ConvertersFabric.GetBroadcastListConverter(SourceObject.Broadcasts)
                    .Convert();

            return postlive;
        }
    }
}