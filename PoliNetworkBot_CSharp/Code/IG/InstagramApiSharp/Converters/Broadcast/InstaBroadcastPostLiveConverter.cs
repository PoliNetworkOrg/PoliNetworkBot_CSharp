#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaBroadcastPostLiveConverter : IObjectConverter<InstaBroadcastPostLive, InstaBroadcastPostLiveResponse>
    {
        public InstaBroadcastPostLiveResponse SourceObject { get; set; }

        public InstaBroadcastPostLive Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var postLive = new InstaBroadcastPostLive
            {
                PeakViewerCount = SourceObject.PeakViewerCount,
                Pk = SourceObject.Pk
            };

            if (SourceObject.User != null)
                postLive.User = ConvertersFabric.GetUserShortFriendshipFullConverter(SourceObject.User).Convert();
            try
            {
                if (SourceObject.Broadcasts?.Count > 0)
                    foreach (var broadcastInfo in SourceObject.Broadcasts)
                        postLive.Broadcasts.Add(ConvertersFabric.GetBroadcastInfoConverter(broadcastInfo)
                            .Convert());
            }
            catch
            {
            }

            return postLive;
        }
    }
}