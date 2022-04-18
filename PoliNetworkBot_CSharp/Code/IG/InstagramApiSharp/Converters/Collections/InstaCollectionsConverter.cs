#region

using System.Collections.Generic;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Converters;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Converters.Collections;

internal class InstaCollectionsConverter : IObjectConverter<InstaCollections, InstaCollectionsResponse>
{
    public InstaCollectionsResponse SourceObject { get; set; }

    public InstaCollections Convert()
    {
        var instaCollectionList = new List<InstaCollectionItem>();
        instaCollectionList.AddRange(SourceObject.Items.Select(ConvertersFabric.GetCollectionConverter)
            .Select(converter => converter.Convert()));

        return new InstaCollections
        {
            Items = instaCollectionList,
            MoreCollectionsAvailable = SourceObject.MoreAvailable,
            NextMaxId = SourceObject.NextMaxId
        };
    }
}