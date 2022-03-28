#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaPlaceListConverter : IObjectConverter<InstaPlaceList, InstaPlaceListResponse>
{
    public InstaPlaceListResponse SourceObject { get; set; }

    public InstaPlaceList Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");

        var list = new InstaPlaceList
        {
            HasMore = SourceObject.HasMore ?? false,
            RankToken = SourceObject.RankToken,
            Status = SourceObject.Status
        };
        if (SourceObject.Items == null || !SourceObject.Items.Any()) return list;
        foreach (var place in SourceObject.Items)
            try
            {
                list.Items.Add(ConvertersFabric.GetPlaceConverter(place).Convert());
            }
            catch
            {
            }

        list.ExcludeList = SourceObject.ExcludeList;

        return list;
    }
}