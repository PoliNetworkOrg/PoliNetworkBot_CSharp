#region

using System.Collections.Generic;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes;

/// <summary>
///     Pagination of everything! use NextMaxId instead of using old NextId
/// </summary>
public class PaginationParameters
{
    private PaginationParameters()
    {
    }

    public string RankToken { get; set; } = string.Empty;
    public string NextMaxId { get; set; } = string.Empty;

    /// <summary>
    ///     Only works for Comments!
    /// </summary>
    public string NextMinId { get; set; } = string.Empty;

    public int MaximumPagesToLoad { get; private init; }
    public int PagesLoaded { get; set; } = 1;

    /// <summary>
    ///     Only for location and hashtag feeds
    /// </summary>
    public int? NextPage { get; set; }

    public List<long> ExcludeList { get; set; } = new();

    /// <summary>
    ///     Only for location and hashtag feeds
    /// </summary>
    public List<long> NextMediaIds { get; set; }

    public static PaginationParameters MaxPagesToLoad(int maxPagesToLoad)
    {
        return new PaginationParameters { MaximumPagesToLoad = maxPagesToLoad };
    }
}