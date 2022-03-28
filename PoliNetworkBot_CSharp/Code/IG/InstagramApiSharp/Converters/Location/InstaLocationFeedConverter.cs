#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaLocationFeedConverter : IObjectConverter<InstaLocationFeed, InstaLocationFeedResponse>
    {
        public InstaLocationFeedResponse SourceObject { get; set; }

        public InstaLocationFeed Convert()
        {
            if (SourceObject == null)
                throw new ArgumentNullException("SourceObject");

            static InstaMediaList ConvertMedia(List<InstaMediaItemResponse> mediasResponse)
            {
                var medias = new InstaMediaList();
                if (mediasResponse == null)
                    return medias;
                medias.AddRange(from instaUserFeedItemResponse in mediasResponse
                                where instaUserFeedItemResponse?.Type == 0
                                select ConvertersFabric.GetSingleMediaConverter(instaUserFeedItemResponse)
                                    .Convert());

                return medias;
            }

            var feed = new InstaLocationFeed
            {
                MediaCount = SourceObject.MediaCount,
                NextMaxId = SourceObject.NextMaxId,
                Medias = ConvertMedia(SourceObject.Items),
                RankedMedias = ConvertMedia(SourceObject.RankedItems),
                Location = ConvertersFabric.GetLocationConverter(SourceObject.Location).Convert(),
                Story = ConvertersFabric.GetStoryConverter(SourceObject.Story).Convert()
            };
            return feed;
        }
    }
}