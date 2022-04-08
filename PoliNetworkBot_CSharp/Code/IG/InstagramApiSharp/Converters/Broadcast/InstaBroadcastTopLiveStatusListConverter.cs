#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaBroadcastTopLiveStatusListConverter : IObjectConverter<InstaBroadcastTopLiveStatusList,
    InstaBroadcastTopLiveStatusResponse>
{
    public InstaBroadcastTopLiveStatusResponse SourceObject { get; set; }

    public InstaBroadcastTopLiveStatusList Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var broadcastStatusItems = new InstaBroadcastTopLiveStatusList();
        try
        {
            if (SourceObject.BroadcastStatusItems?.Count > 0)
                broadcastStatusItems.AddRange(SourceObject.BroadcastStatusItems.Select(statusItem =>
                    ConvertersFabric.GetBroadcastStatusItemConverter(statusItem)
                        .Convert()));
        }
        catch
        {
        }

        return broadcastStatusItems;
    }
}