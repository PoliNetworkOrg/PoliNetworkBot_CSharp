#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaHighlightShortConverter : IObjectConverter<InstaHighlightShort, InstaHighlightShortResponse>
    {
        public InstaHighlightShortResponse SourceObject { get; set; }

        public InstaHighlightShort Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var highlight = new InstaHighlightShort
            {
                Id = SourceObject.Id,
                LatestReelMedia = SourceObject.LatestReelMedia,
                MediaCount = SourceObject.MediaCount,
                ReelType = SourceObject.ReelType,
                Time = (SourceObject.Timestamp ?? 0).FromUnixTimeSeconds()
            };
            return highlight;
        }
    }
}