#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Enums;
using InstagramApiSharp.Helpers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaVisualMediaContainerConverter : IObjectConverter<InstaVisualMediaContainer,
            InstaVisualMediaContainerResponse>
    {
        public InstaVisualMediaContainerResponse SourceObject { get; set; }

        public InstaVisualMediaContainer Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var visualMedia = new InstaVisualMediaContainer
            {
                SeenCount = SourceObject.SeenCount ?? 0
            };

            if (SourceObject.UrlExpireAtSecs != null)
                visualMedia.UrlExpireAt = SourceObject.UrlExpireAtSecs.Value.FromUnixTimeSeconds();

            if (SourceObject.ReplayExpiringAtUs != null)
                visualMedia.ReplayExpiringAtUs =
                    DateTime.MinValue /*SourceObject.ReplayExpiringAtUs.Value.FromUnixTimeSeconds()*/;

            if (SourceObject.Media != null)
                visualMedia.Media = ConvertersFabric.Instance.GetVisualMediaConverter(SourceObject.Media).Convert();

            if (!string.IsNullOrEmpty(SourceObject.ViewMode))
                visualMedia.ViewMode = (InstaViewMode)Enum.Parse(typeof(InstaViewMode), SourceObject.ViewMode, true);

            if (!(SourceObject.SeenUserIds?.Count > 0)) return visualMedia;
            foreach (var user in SourceObject.SeenUserIds)
                visualMedia.SeenUserIds.Add(user);

            return visualMedia;
        }
    }
}