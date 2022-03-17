#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaCarouselConverter : IObjectConverter<InstaCarousel, InstaCarouselResponse>
    {
        public InstaCarouselResponse SourceObject { get; set; }

        public InstaCarousel Convert()
        {
            var carousel = new InstaCarousel();
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            carousel.AddRange(SourceObject.Select(item => ConvertersFabric.Instance.GetCarouselItemConverter(item))
                .Select(carouselItem => carouselItem.Convert()));

            return carousel;
        }
    }
}