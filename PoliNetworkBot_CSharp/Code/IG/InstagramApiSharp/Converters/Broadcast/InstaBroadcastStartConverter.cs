#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaBroadcastStartConverter : IObjectConverter<InstaBroadcastStart, InstaBroadcastStartResponse>
{
    public InstaBroadcastStartResponse SourceObject { get; set; }

    public InstaBroadcastStart Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var broadcastStart = new InstaBroadcastStart
        {
            MediaId = SourceObject.MediaId
        };

        return broadcastStart;
    }
}