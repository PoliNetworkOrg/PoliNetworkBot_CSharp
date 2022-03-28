#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaDiscoverRecentSearches
{
    public List<InstaDiscoverSearches> Recent { get; set; } = new();
}