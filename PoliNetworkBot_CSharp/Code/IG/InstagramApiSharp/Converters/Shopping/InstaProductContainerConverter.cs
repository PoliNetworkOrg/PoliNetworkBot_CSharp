#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Media;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaProductContainerConverter : IObjectConverter<InstaProductTag, InstaProductContainerResponse>
{
    public InstaProductContainerResponse SourceObject { get; set; }

    public InstaProductTag Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var productTag = new InstaProductTag
        {
            Product = ConvertersFabric.GetProductConverter(SourceObject.Product).Convert()
        };

        if (SourceObject.Position != null)
            productTag.Position = new InstaPosition(SourceObject.Position[0], SourceObject.Position[1]);

        return productTag;
    }
}