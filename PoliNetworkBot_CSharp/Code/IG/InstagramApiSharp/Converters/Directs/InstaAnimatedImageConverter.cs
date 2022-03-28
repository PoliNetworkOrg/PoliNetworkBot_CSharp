#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaAnimatedImageConverter : IObjectConverter<InstaAnimatedImage, InstaAnimatedImageResponse>
    {
        public InstaAnimatedImageResponse SourceObject { get; set; }

        public InstaAnimatedImage Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var animatedImage = new InstaAnimatedImage
            {
                Id = SourceObject.Id,
                IsRandom = SourceObject.IsRandom ?? false,
                IsSticker = SourceObject.IsSticker ?? false
            };

            if (SourceObject.Images is { Media: { } })
                animatedImage.Media = ConvertersFabric.GetAnimatedImageMediaConverter(SourceObject.Images.Media).Convert();

            if (SourceObject.User != null)
                animatedImage.User =
                    ConvertersFabric.GetAnimatedImageUserConverter(SourceObject.User).Convert();

            return animatedImage;
        }
    }
}