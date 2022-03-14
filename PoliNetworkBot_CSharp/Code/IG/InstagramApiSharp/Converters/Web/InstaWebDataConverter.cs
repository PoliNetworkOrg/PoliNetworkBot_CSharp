#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers.Web;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaWebDataConverter : IObjectConverter<InstaWebData, InstaWebSettingsPageResponse>
    {
        public InstaWebSettingsPageResponse SourceObject { get; set; }

        public InstaWebData Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var data = new InstaWebData();

            if (!(SourceObject.Data?.Data?.Count > 0)) return data;
            foreach (var item in SourceObject.Data.Data)
                data.Items.Add(ConvertersFabric.Instance.GetWebDataItemConverter(item).Convert());

            data.MaxId = SourceObject.Data.Cursor;

            return data;
        }
    }
}