#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaExploreFeedConverter : IObjectConverter<InstaExploreFeed, InstaExploreFeedResponse>
{
    public InstaExploreFeedResponse SourceObject { get; set; }

    public InstaExploreFeed Convert()
    {
        if (SourceObject == null)
            throw new ArgumentNullException("SourceObject");

        static IEnumerable<InstaMedia> ConvertMedia(List<InstaMediaItemResponse> mediasResponse)
        {
            var medias = new List<InstaMedia>();
            if (mediasResponse == null)
                return medias;
            medias.AddRange(from instaUserFeedItemResponse in mediasResponse
                            where instaUserFeedItemResponse?.Type == 0
                            select ConvertersFabric.GetSingleMediaConverter(instaUserFeedItemResponse)
                                .Convert());

            return medias;
        }

        var feed = new InstaExploreFeed
        {
            NextMaxId = SourceObject.NextMaxId,
            AutoLoadMoreEnabled = SourceObject.AutoLoadMoreEnabled,
            ResultsCount = SourceObject.ResultsCount,
            MoreAvailable = SourceObject.MoreAvailable,
            MaxId = SourceObject.MaxId,
            RankToken = SourceObject.RankToken
        };
        if (SourceObject.Items?.StoryTray != null)
            feed.StoryTray = ConvertersFabric.GetStoryTrayConverter(SourceObject.Items.StoryTray)
                .Convert();
        if (SourceObject.Items?.Channel != null)
            feed.Channel = ConvertersFabric.GetChannelConverter(SourceObject.Items.Channel).Convert();

        feed.Medias.AddRange(ConvertMedia(SourceObject.Items?.Medias));
        return feed;
    }
}