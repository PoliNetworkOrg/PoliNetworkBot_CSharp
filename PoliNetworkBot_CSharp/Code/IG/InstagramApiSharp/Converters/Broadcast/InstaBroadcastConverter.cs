#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaBroadcastConverter : IObjectConverter<InstaBroadcast, InstaBroadcastResponse>
    {
        public InstaBroadcastResponse SourceObject { get; set; }

        public InstaBroadcast Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var broadcast = new InstaBroadcast
            {
                DashManifest = SourceObject.DashManifest,
                BroadcastMessage = SourceObject.BroadcastMessage,
                BroadcastStatus = SourceObject.BroadcastStatus,
                CoverFrameUrl = SourceObject.CoverFrameUrl,
                DashAbrPlaybackUrl = SourceObject.DashAbrPlaybackUrl,
                DashPlaybackUrl = SourceObject.DashPlaybackUrl,
                Id = SourceObject.Id,
                InternalOnly = SourceObject.InternalOnly,
                MediaId = SourceObject.MediaId,
                OrganicTrackingToken = SourceObject.OrganicTrackingToken,
                PublishedTime = (SourceObject.PublishedTime ?? DateTime.Now.ToUnixTime()).FromUnixTimeSeconds(),
                RtmpPlaybackUrl = SourceObject.RtmpPlaybackUrl,
                ViewerCount = SourceObject.ViewerCount
            };

            if (SourceObject.BroadcastOwner != null)
                broadcast.BroadcastOwner = ConvertersFabric.Instance
                    .GetUserShortFriendshipFullConverter(SourceObject.BroadcastOwner).Convert();
            return broadcast;
        }
    }
}