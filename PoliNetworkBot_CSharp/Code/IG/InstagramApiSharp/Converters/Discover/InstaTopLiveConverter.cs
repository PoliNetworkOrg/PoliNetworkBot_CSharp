#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaTopLiveConverter : IObjectConverter<InstaTopLive, InstaTopLiveResponse>
    {
        public InstaTopLiveResponse SourceObject { get; set; }

        public InstaTopLive Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var storyTray = new InstaTopLive { RankedPosition = SourceObject.RankedPosition };
            foreach (var owner in SourceObject.BroadcastOwners)
            {
                var userOwner = ConvertersFabric.Instance.GetUserShortFriendshipFullConverter(owner).Convert();
                storyTray.BroadcastOwners.Add(userOwner);
            }

            return storyTray;
        }
    }
}