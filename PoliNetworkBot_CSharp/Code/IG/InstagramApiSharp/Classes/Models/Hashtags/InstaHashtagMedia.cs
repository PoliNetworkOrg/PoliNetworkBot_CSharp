#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Hashtags;

public class InstaSectionMedia
{
    public List<InstaMedia> Medias { get; set; } = new();

    public List<InstaRelatedHashtag> RelatedHashtags { get; set; } = new();

    public bool MoreAvailable { get; set; }

    public string? NextMaxId { get; set; }

    public int NextPage { get; set; }

    public List<long> NextMediaIds { get; set; } = new();

    public bool AutoLoadMoreEnabled { get; set; }
}

/*public class InstaHashtagMedia
{
    public string LayoutType { get; set; }

    public List<InstaMedia> Medias { get; set; } = new List<InstaMedia>();

    public string FeedType { get; set; }

    public InstaHashtagMediaExploreItemInfo ExploreItemInfo { get; set; }
}
public class InstaHashtagMediaExploreItemInfo
{
    public int NumBolumns { get; set; }

    public int TotalNumBolumns { get; set; }

    public int AspectYatio { get; set; }

    public bool Autoplay { get; set; }
}*/