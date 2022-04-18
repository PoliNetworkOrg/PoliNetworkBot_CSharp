#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Converters;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Converters.Broadcast;

internal class
    InstaBroadcastAddToPostLiveConverter : IObjectConverter<InstaBroadcastAddToPostLive,
        InstaBroadcastAddToPostLiveResponse>
{
    public InstaBroadcastAddToPostLiveResponse SourceObject { get; init; }

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