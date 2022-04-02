#region

using InstagramApiSharp.Classes.ResponseWrappers.Business;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Business;

#endregion

namespace InstagramApiSharp.Converters.Business;

internal class
    InstaStatisticsDataPointConverter : IObjectConverter<InstaStatisticsDataPointItem,
        InstaStatisticsDataPointItemResponse>
{
    public InstaStatisticsDataPointItemResponse SourceObject { get; set; }

    public InstaStatisticsDataPointItem Convert()
    {
        var dataPoint = new InstaStatisticsDataPointItem();
        return dataPoint;
    }
}