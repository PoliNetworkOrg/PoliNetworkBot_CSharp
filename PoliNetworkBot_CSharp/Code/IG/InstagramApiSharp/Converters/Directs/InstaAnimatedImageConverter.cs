#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

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

            if (SourceObject.Images != null && SourceObject.Images?.Media != null)
                animatedImage.Media = ConvertersFabric.Instance
                    .GetAnimatedImageMediaConverter(SourceObject.Images.Media).Convert();

            if (SourceObject.User != null)
                animatedImage.User =
                    ConvertersFabric.Instance.GetAnimatedImageUserConverter(SourceObject.User).Convert();

            return animatedImage;
        }
    }
}