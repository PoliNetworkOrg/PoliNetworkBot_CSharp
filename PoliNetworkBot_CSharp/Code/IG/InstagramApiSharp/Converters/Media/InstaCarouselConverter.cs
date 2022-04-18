#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaCarouselConverter : IObjectConverter<InstaCarousel, InstaCarouselResponse>
{
    public InstaCarouselResponse SourceObject { get; set; }

    public InstaCarousel Convert()
    {
        var carousel = new InstaCarousel();
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        carousel.AddRange(SourceObject.Select(ConvertersFabric.GetCarouselItemConverter)
            .Select(carouselItem => carouselItem.Convert()));

        return carousel;
    }
}