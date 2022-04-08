#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;
using System;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaBroadcastInfoConverter : IObjectConverter<InstaBroadcastInfo, InstaBroadcastInfoResponse>
{
    public InstaBroadcastInfoResponse SourceObject { get; set; }

    public InstaBroadcastInfo Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");

        var unixTime = DateTime.Now.ToUnixTime();
        var broadcastInfo = new InstaBroadcastInfo
        {
            BroadcastMessage = SourceObject.BroadcastMessage,
            BroadcastStatus = SourceObject.BroadcastStatus,
            CoverFrameUrl = SourceObject.CoverFrameUrl,
            DashManifest = SourceObject.DashManifest,
            EncodingTag = SourceObject.EncodingTag,
            Id = SourceObject.Id,
            InternalOnly = SourceObject.InternalOnly,
            MediaId = SourceObject.MediaId,
            NumberOfQualities = SourceObject.NumberOfQualities,
            OrganicTrackingToken = SourceObject.OrganicTrackingToken,
            TotalUniqueViewerCount = SourceObject.TotalUniqueViewerCount,
            ExpireAt = (SourceObject.ExpireAt ?? unixTime).FromUnixTimeSeconds(),
            PublishedTime = (SourceObject.PublishedTime ?? unixTime).FromUnixTimeSeconds()
        };

        if (SourceObject.BroadcastOwner != null)
            broadcastInfo.BroadcastOwner =
                ConvertersFabric.GetUserShortFriendshipFullConverter(SourceObject.BroadcastOwner).Convert();
        return broadcastInfo;
    }
}