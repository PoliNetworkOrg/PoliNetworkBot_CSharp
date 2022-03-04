#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaBroadcastListConverter : IObjectConverter<InstaBroadcastList, List<InstaBroadcastResponse>>
    {
        public List<InstaBroadcastResponse> SourceObject { get; set; }

        public InstaBroadcastList Convert()
        {
            //if (SourceObject == null) throw new ArgumentNullException($"Source object");
            var broadcastList = new InstaBroadcastList();
            if (SourceObject?.Count > 0)
                foreach (var broadcast in SourceObject)
                    broadcastList.Add(ConvertersFabric.Instance.GetBroadcastConverter(broadcast).Convert());

            return broadcastList;
        }
    }
}