#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaDiscoverTopLiveConverter : IObjectConverter<InstaDiscoverTopLive, InstaDiscoverTopLiveResponse>
{
    public InstaDiscoverTopLiveResponse SourceObject { get; set; }

    public InstaDiscoverTopLive Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var discoverTopLive = new InstaDiscoverTopLive
        {
            AutoLoadMoreEnabled = SourceObject.AutoLoadMoreEnabled,
            MoreAvailable = SourceObject.MoreAvailable,
            NextMaxId = SourceObject.NextMaxId
        };

        if (SourceObject.Broadcasts?.Count > 0)
            discoverTopLive.Broadcasts = ConvertersFabric.GetBroadcastListConverter(SourceObject.Broadcasts).Convert();

        if (!(SourceObject.PostLiveBroadcasts?.Count > 0)) return discoverTopLive;
        foreach (var postLive in SourceObject.PostLiveBroadcasts)
            discoverTopLive.PostLiveBroadcasts.Add(ConvertersFabric.GetBroadcastPostLiveConverter(postLive).Convert());

        return discoverTopLive;
    }
}