#region

using System;
using System.Collections.Generic;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaTagFeedConverter : IObjectConverter<InstaTagFeed, InstaTagFeedResponse>
    {
        public InstaTagFeedResponse SourceObject { get; set; }

        public InstaTagFeed Convert()
        {
            if (SourceObject?.Medias == null)
                throw new ArgumentNullException("InstaFeedResponse or its media list");
            var feed = new InstaTagFeed();

            IEnumerable<InstaMedia> ConvertMedia(IEnumerable<InstaMediaItemResponse> mediasResponse)
            {
                return (from instaUserFeedItemResponse in mediasResponse
                    where instaUserFeedItemResponse?.Type == 0
                    select ConvertersFabric.Instance.GetSingleMediaConverter(instaUserFeedItemResponse)
                        .Convert()).ToList();
            }

            feed.RankedMedias.AddRange(ConvertMedia(SourceObject.RankedItems));
            feed.Medias.AddRange(ConvertMedia(SourceObject.Medias));
            feed.NextMaxId = SourceObject.NextMaxId;
            foreach (var feedItem in SourceObject.Stories.Select(story =>
                         ConvertersFabric.Instance.GetStoryConverter(story).Convert())) feed.Stories.Add(feedItem);

            return feed;
        }
    }
}