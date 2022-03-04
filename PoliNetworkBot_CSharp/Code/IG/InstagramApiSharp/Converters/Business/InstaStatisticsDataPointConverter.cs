#region

using InstagramApiSharp.Classes.Models.Business;
using InstagramApiSharp.Classes.ResponseWrappers.Business;

#endregion

namespace InstagramApiSharp.Converters.Business
{
    internal class
        InstaStatisticsDataPointConverter : IObjectConverter<InstaStatisticsDataPointItem,
            InstaStatisticsDataPointItemResponse>
    {
        public InstaStatisticsDataPointItemResponse SourceObject { get; set; }

        public InstaStatisticsDataPointItem Convert()
        {
            var dataPoint = new InstaStatisticsDataPointItem
            {
                Label = SourceObject.Label,
                Value = SourceObject.Value ?? 0
            };
            return dataPoint;
        }
    }
}