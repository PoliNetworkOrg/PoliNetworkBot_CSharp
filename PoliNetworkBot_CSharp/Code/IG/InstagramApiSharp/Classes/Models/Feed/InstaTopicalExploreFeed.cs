#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaTopicalExploreFeed
{
    public List<InstaTopicalExploreCluster> Clusters { get; set; } = new();

    public InstaMediaList Medias { get; set; } = new();

    public string NextMaxId { get; set; }

    public List<InstaTVChannel> TVChannels { get; set; } = new();

    public InstaChannel Channel { get; set; } = new();

    public string MaxId { get; set; }

    public string RankToken { get; set; }

    public bool MoreAvailable { get; set; }

    public int ResultsCount { get; set; }

    public bool AutoLoadMoreEnabled { get; set; }

    public bool HasShoppingChannelContent { get; set; }
}