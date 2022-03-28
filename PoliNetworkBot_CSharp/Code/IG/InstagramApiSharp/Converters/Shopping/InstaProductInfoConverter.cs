#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaProductInfoConverter : IObjectConverter<InstaProductInfo, InstaProductInfoResponse>
    {
        public InstaProductInfoResponse SourceObject { get; set; }

        public InstaProductInfo Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var productInfo = new InstaProductInfo
            {
                Product = ConvertersFabric.Instance.GetProductConverter(SourceObject.Product).Convert(),
                User = ConvertersFabric.GetUserShortConverter(SourceObject.User).Convert()
            };
            if (SourceObject.OtherProductItems != null && SourceObject.OtherProductItems.Any())
                foreach (var product in SourceObject.OtherProductItems)
                    productInfo.OtherProducts.Add(ConvertersFabric.Instance.GetProductConverter(product).Convert());

            if (SourceObject.MoreFromBusiness != null)
                productInfo.MoreFromBusiness = ConvertersFabric.Instance
                    .GetProductMediaListConverter(SourceObject.MoreFromBusiness).Convert();
            return productInfo;
        }
    }
}