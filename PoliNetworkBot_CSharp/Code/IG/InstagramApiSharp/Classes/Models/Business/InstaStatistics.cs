#region

using System.Collections.Generic;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Business;

public class InstaStatistics
{
    public InstaStatisticsBusinessManager BusinessManager { get; set; }
}

public class InstaStatisticsBusinessManager
{
    public InstaStatisticsAccountInsightsUnit AccountInsightsUnit { get; set; }

    public InstaStatisticsFollowersUnit FollowersUnit { get; set; }

    public InstaStatisticsTopPostsUnit TopPostsUnit { get; set; }
}

public class InstaStatisticsSummaryPromotions
{
}

public class InstaStatisticsAccountSummaryUnit
{
}

public class InstaStatisticsStoriesUnit
{
}

public class InstaStatisticsSummaryStories : InstaStatisticsSummaryPromotions
{
}

public class InstaStatisticsTopPostsUnit
{
    public List<InstaMediaShort> SummaryPosts { get; } = new();

    public List<InstaMediaShort> TopPosts { get; } = new();
}

public class InstaMediaShort
{
}

public class InstaStatisticsDataPointItem
{
}

public class InstaStatisticsFollowersUnit
{
    public List<InstaStatisticsDataPointItem> GenderGraph { get; set; } = new();

    public List<InstaStatisticsDataPointItem> AllFollowersAgeGraph { get; set; } = new();

    public List<InstaStatisticsDataPointItem> MenFollowersAgeGraph { get; set; } = new();

    public List<InstaStatisticsDataPointItem> WomenFollowersAgeGraph { get; set; } = new();

    public List<InstaStatisticsDataPointItem> FollowersTopCitiesGraph { get; set; } = new();

    public List<InstaStatisticsDataPointItem> FollowersTopCountriesGraph { get; set; } = new();

    public List<InstaStatisticsDataPointItem> WeekDailyFollowersGraph { get; set; } = new();

    public List<InstaStatisticsDataPointItem> DaysHourlyFollowersGraphs { get; set; } = new();

    public List<InstaStatisticsDataPointItem> TodayHourlyGraph { get; set; } = new();
}

public class InstaStatisticsAccountInsightsUnit
{
    public List<InstaStatisticsDataPointItem> AccountActionsLastWeekDailyGraph { get; set; } = new();

    public List<InstaStatisticsDataPointItem> AccountDiscoveryLastWeekDailyGraph { get; set; } = new();
}

public class InstaStatisticsInsightsChannel
{
}