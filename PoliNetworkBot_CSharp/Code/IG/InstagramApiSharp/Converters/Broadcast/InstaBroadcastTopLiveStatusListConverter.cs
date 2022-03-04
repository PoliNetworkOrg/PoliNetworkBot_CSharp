#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
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
                    foreach (var statusItem in SourceObject.BroadcastStatusItems)
                        broadcastStatusItems.Add(ConvertersFabric.Instance.GetBroadcastStatusItemConverter(statusItem)
                            .Convert());
            }
            catch
            {
            }

            return broadcastStatusItems;
        }
    }
}