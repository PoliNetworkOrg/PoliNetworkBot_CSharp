﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Feed;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.ResponseWrappers.Feed;

#endregion

namespace InstagramApiSharp.Converters;

internal class
    InstaTopicalExploreFeedConverter : IObjectConverter<InstaTopicalExploreFeed, InstaTopicalExploreFeedResponse>
{
    public InstaTopicalExploreFeedResponse SourceObject { get; set; }

    public InstaTopicalExploreFeed Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("SourceObject");

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

        var feed = new InstaTopicalExploreFeed
        {
            NextMaxId = SourceObject.NextMaxId,
            AutoLoadMoreEnabled = SourceObject.AutoLoadMoreEnabled,
            ResultsCount = SourceObject.ResultsCount,
            MoreAvailable = SourceObject.MoreAvailable,
            MaxId = SourceObject.MaxId,
            RankToken = SourceObject.RankToken,
            HasShoppingChannelContent = SourceObject.HasShoppingChannelContent ?? false
        };
        if (SourceObject.TvChannels?.Count > 0)
            foreach (var channel in SourceObject.TvChannels)
                try
                {
                    feed.TvChannels.Add(ConvertersFabric.GetTvChannelConverter(channel).Convert());
                }
                catch
                {
                }

        if (SourceObject.Clusters?.Count > 0)
            foreach (var cluster in SourceObject.Clusters)
                try
                {
                    feed.Clusters.Add(ConvertersFabric.GetExploreClusterConverter(cluster).Convert());
                }
                catch
                {
                }

        if (SourceObject.Channel != null)
            feed.Channel = ConvertersFabric.GetChannelConverter(SourceObject.Channel).Convert();

        feed.Medias.AddRange(ConvertMedia(SourceObject.Medias));
        return feed;
    }
}