#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

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
                 select ConvertersFabric.GetSingleMediaConverter(instaUserFeedItemResponse).Convert())
            feed.Medias.Add(feedItem);

        foreach (var suggestedItemResponse in SourceObject.SuggestedUsers)
            try
            {
                var suggestedItem = ConvertersFabric.GetSuggestionItemConverter(suggestedItemResponse)
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