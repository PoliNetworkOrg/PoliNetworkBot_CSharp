﻿#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaDiscoverTopSearches
{
    public string RankToken { get; set; }

    public List<InstaDiscoverSearches> TopResults { get; set; } = new();
}