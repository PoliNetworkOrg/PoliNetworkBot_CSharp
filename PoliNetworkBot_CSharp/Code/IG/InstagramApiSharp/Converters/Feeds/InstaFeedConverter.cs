#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaFeedConverter : IObjectConverter<InstaFeed, InstaFeedResponse>
    {
        public InstaFeedResponse SourceObject { get; set; }

        public InstaFeed Convert()
        {
            if (SourceObject?.Items == null)
                throw new ArgumentNullException("InstaFeedResponse or its Items");
            var feed = new InstaFeed();
            foreach (var feedItem in from instaUserFeedItemResponse in SourceObject.Items
                                     where instaUserFeedItemResponse?.Type == 0
                                     select ConvertersFabric.Instance.GetSingleMediaConverter(instaUserFeedItemResponse).Convert())
                feed.Medias.Add(feedItem);

            foreach (var suggestedItemResponse in SourceObject.SuggestedUsers)
                try
                {
                    var suggestedItem = ConvertersFabric.Instance.GetSuggestionItemConverter(suggestedItemResponse)
                        .Convert();
                    feed.SuggestedUserItems.Add(suggestedItem);
                }
                catch
                {
                }

            feed.NextMaxId = SourceObject.NextMaxId;
            return feed;
        }
    }
}