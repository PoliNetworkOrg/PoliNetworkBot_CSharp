#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaTopLiveConverter : IObjectConverter<InstaTopLive, InstaTopLiveResponse>
{
    public InstaTopLiveResponse SourceObject { get; set; }

    public InstaTopLive Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var storyTray = new InstaTopLive { RankedPosition = SourceObject.RankedPosition };
        foreach (var userOwner in SourceObject.BroadcastOwners.Select(owner =>
                     ConvertersFabric.GetUserShortFriendshipFullConverter(owner).Convert()))
            storyTray.BroadcastOwners.Add(userOwner);

        return storyTray;
    }
}