#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.TV;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Feed;

public class InstaTopicalExploreFeed
{
    public List<InstaTopicalExploreCluster> Clusters { get; set; } = new();

    public InstaMediaList Medias { get; set; } = new();

    public string NextMaxId { get; set; }

    public List<InstaTvChannel> TvChannels { get; set; } = new();

    public InstaChannel Channel { get; set; } = new();

    public string MaxId { get; set; }

    public string RankToken { get; set; }

    public bool MoreAvailable { get; set; }

    public int ResultsCount { get; set; }

    public bool AutoLoadMoreEnabled { get; set; }

    public bool HasShoppingChannelContent { get; set; }
}