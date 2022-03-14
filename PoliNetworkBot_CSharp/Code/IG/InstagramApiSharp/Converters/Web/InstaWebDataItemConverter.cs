#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers.Web;
using InstagramApiSharp.Helpers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaWebDataItemConverter : IObjectConverter<InstaWebDataItem, InstaWebDataItemResponse>
    {
        public InstaWebDataItemResponse SourceObject { get; set; }

        public InstaWebDataItem Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var data = new InstaWebDataItem
            {
                Text = SourceObject.Text
            };

            data.Time = SourceObject.Timestamp != null ? SourceObject.Timestamp.Value.FromUnixTimeSeconds() : DateTime.MinValue;

            return data;
        }
    }
}