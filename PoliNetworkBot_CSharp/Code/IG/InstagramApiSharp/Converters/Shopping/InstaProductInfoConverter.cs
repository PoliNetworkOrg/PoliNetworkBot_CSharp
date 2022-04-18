#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaProductInfoConverter : IObjectConverter<InstaProductInfo, InstaProductInfoResponse>
{
    public InstaProductInfoResponse SourceObject { get; set; }

    public InstaProductInfo Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var productInfo = new InstaProductInfo
        {
            Product = ConvertersFabric.GetProductConverter(SourceObject.Product).Convert(),
            User = ConvertersFabric.GetUserShortConverter(SourceObject.User).Convert()
        };
        if (SourceObject.OtherProductItems != null && SourceObject.OtherProductItems.Any())
            foreach (var product in SourceObject.OtherProductItems)
                productInfo.OtherProducts.Add(ConvertersFabric.GetProductConverter(product).Convert());

        if (SourceObject.MoreFromBusiness != null)
            productInfo.MoreFromBusiness =
                ConvertersFabric.GetProductMediaListConverter(SourceObject.MoreFromBusiness).Convert();
        return productInfo;
    }
}