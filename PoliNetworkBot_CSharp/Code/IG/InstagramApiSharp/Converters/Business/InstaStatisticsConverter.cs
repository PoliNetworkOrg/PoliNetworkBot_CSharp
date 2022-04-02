#region

using InstagramApiSharp.Classes.ResponseWrappers.Business;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Business;

#endregion

namespace InstagramApiSharp.Converters.Business;

internal class InstaStatisticsConverter : IObjectConverter<InstaStatistics, InstaStatisticsRootResponse>
{
    public InstaStatisticsRootResponse SourceObject { get; set; }

    public InstaStatistics Convert()
    {
        if (SourceObject?.Data?.User == null)
            return null;
        var user = SourceObject.Data.User;
        var statisfics = new InstaStatistics();
        if (user.BusinessProfile is { Id: { } })
        {
        }

        if (user.ProfilePicture is { Uri: { } })
        {
        }

        statisfics.BusinessManager = new InstaStatisticsBusinessManager();

        var businessManager = user.BusinessManager;

        if (businessManager.PromotionsUnit is { SummaryPromotions: { } })
            try
            {
                new InstaStatisticsSummaryPromotions();
            }
            catch
            {
            }

        if (businessManager.AccountSummaryUnit is { })
            try
            {
                new InstaStatisticsAccountSummaryUnit();
            }
            catch
            {
            }

        if (businessManager.StoriesUnit != null)
            try
            {
                var storyUnit = new InstaStatisticsStoriesUnit();
                if (businessManager.StoriesUnit.SummaryStories != null)
                    new InstaStatisticsSummaryStories();
            }
            catch
            {
            }

        if (businessManager.TopPostsUnit != null)
            try
            {
                statisfics.BusinessManager.TopPostsUnit = new InstaStatisticsTopPostsUnit();
                if (businessManager.TopPostsUnit.SummaryPosts != null)
                    foreach (var media in businessManager.TopPostsUnit.SummaryPosts.Edges)
                        try
                        {
                            var convertedMedia = ConvertersFabric.GetMediaShortConverter(media.Node)
                                .Convert();
                            statisfics.BusinessManager.TopPostsUnit.SummaryPosts.Add(convertedMedia);
                        }
                        catch
                        {
                        }

                if (businessManager.TopPostsUnit.TopPosts != null)
                    foreach (var media in businessManager.TopPostsUnit.TopPosts.Edges)
                        try
                        {
                            var convertedMedia = ConvertersFabric.GetMediaShortConverter(media.Node)
                                .Convert();
                            statisfics.BusinessManager.TopPostsUnit.TopPosts.Add(convertedMedia);
                        }
                        catch
                        {
                        }
            }
            catch
            {
            }

        if (businessManager.FollowersUnit != null)
            try
            {
                statisfics.BusinessManager.FollowersUnit = new InstaStatisticsFollowersUnit();
                foreach (var dataPoint in businessManager.FollowersUnit.AllFollowersAgeGraph.DataPoints)
                    try
                    {
                        var convertedDataPoint = ConvertersFabric.GetStatisticsDataPointConverter(dataPoint).Convert();
                        statisfics.BusinessManager.FollowersUnit.AllFollowersAgeGraph.Add(convertedDataPoint);
                    }
                    catch
                    {
                    }

                foreach (var graph in businessManager.FollowersUnit.DaysHourlyFollowersGraphs)
                foreach (var dataPoint in graph.DataPoints)
                    try
                    {
                        var convertedDataPoint = ConvertersFabric.GetStatisticsDataPointConverter(dataPoint).Convert();
                        statisfics.BusinessManager.FollowersUnit.DaysHourlyFollowersGraphs.Add(convertedDataPoint);
                    }
                    catch
                    {
                    }

                foreach (var dataPoint in businessManager.FollowersUnit.FollowersTopCitiesGraph.DataPoints)
                    try
                    {
                        var convertedDataPoint = ConvertersFabric.GetStatisticsDataPointConverter(dataPoint).Convert();
                        statisfics.BusinessManager.FollowersUnit.FollowersTopCitiesGraph.Add(convertedDataPoint);
                    }
                    catch
                    {
                    }

                foreach (var dataPoint in businessManager.FollowersUnit.FollowersTopCountriesGraph.DataPoints)
                    try
                    {
                        var convertedDataPoint = ConvertersFabric.GetStatisticsDataPointConverter(dataPoint).Convert();
                        statisfics.BusinessManager.FollowersUnit.FollowersTopCountriesGraph.Add(convertedDataPoint);
                    }
                    catch
                    {
                    }

                foreach (var dataPoint in businessManager.FollowersUnit.GenderGraph.DataPoints)
                    try
                    {
                        var convertedDataPoint = ConvertersFabric.GetStatisticsDataPointConverter(dataPoint).Convert();
                        statisfics.BusinessManager.FollowersUnit.GenderGraph.Add(convertedDataPoint);
                    }
                    catch
                    {
                    }

                foreach (var dataPoint in businessManager.FollowersUnit.MenFollowersAgeGraph.DataPoints)
                    try
                    {
                        var convertedDataPoint = ConvertersFabric.GetStatisticsDataPointConverter(dataPoint).Convert();
                        statisfics.BusinessManager.FollowersUnit.MenFollowersAgeGraph.Add(convertedDataPoint);
                    }
                    catch
                    {
                    }

                foreach (var dataPoint in businessManager.FollowersUnit.TodayHourlyGraph.DataPoints)
                    try
                    {
                        var convertedDataPoint = ConvertersFabric.GetStatisticsDataPointConverter(dataPoint).Convert();
                        statisfics.BusinessManager.FollowersUnit.TodayHourlyGraph.Add(convertedDataPoint);
                    }
                    catch
                    {
                    }

                foreach (var dataPoint in businessManager.FollowersUnit.WeekDailyFollowersGraph.DataPoints)
                    try
                    {
                        var convertedDataPoint = ConvertersFabric.GetStatisticsDataPointConverter(dataPoint).Convert();
                        statisfics.BusinessManager.FollowersUnit.WeekDailyFollowersGraph.Add(convertedDataPoint);
                    }
                    catch
                    {
                    }

                foreach (var dataPoint in businessManager.FollowersUnit.WomenFollowersAgeGraph.DataPoints)
                    try
                    {
                        var convertedDataPoint = ConvertersFabric.GetStatisticsDataPointConverter(dataPoint).Convert();
                        statisfics.BusinessManager.FollowersUnit.WomenFollowersAgeGraph.Add(convertedDataPoint);
                    }
                    catch
                    {
                    }
            }
            catch
            {
            }

        if (businessManager.AccountInsightsUnit == null) return statisfics;
        {
            try
            {
                statisfics.BusinessManager.AccountInsightsUnit = new InstaStatisticsAccountInsightsUnit();

                if (businessManager.AccountInsightsUnit.InstagramAccountInsightsChannel != null)
                    try
                    {
                        new InstaStatisticsInsightsChannel();
                    }
                    catch
                    {
                    }

                if (businessManager.AccountInsightsUnit.AccountActionsLastWeekDailyGraph is
                    {
                        TotalCountGraph.DataPoints:
                        { }
                    })
                    foreach (var dataPoint in businessManager.AccountInsightsUnit.AccountActionsLastWeekDailyGraph
                                 .TotalCountGraph.DataPoints)
                        try
                        {
                            var convertedDataPoint =
                                ConvertersFabric.GetStatisticsDataPointConverter(dataPoint).Convert();
                            statisfics.BusinessManager.AccountInsightsUnit.AccountActionsLastWeekDailyGraph.Add(
                                convertedDataPoint);
                        }
                        catch
                        {
                        }

                if (businessManager.AccountInsightsUnit.AccountDiscoveryLastWeekDailyGraph is { Nodes: { } })
                    foreach (var node in businessManager.AccountInsightsUnit.AccountDiscoveryLastWeekDailyGraph
                                 .Nodes)
                    foreach (var dataPoint in node.DataPoints)
                        try
                        {
                            var convertedDataPoint =
                                ConvertersFabric.GetStatisticsDataPointConverter(dataPoint).Convert();
                            statisfics.BusinessManager.AccountInsightsUnit.AccountDiscoveryLastWeekDailyGraph.Add(
                                convertedDataPoint);
                        }
                        catch
                        {
                        }
            }
            catch
            {
            }
        }

        return statisfics;
    }
}