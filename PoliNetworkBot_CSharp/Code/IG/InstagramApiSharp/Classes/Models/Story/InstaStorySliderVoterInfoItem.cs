#region

using System;
using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Story;

public class InstaStorySliderVoterInfoItem
{
    public long SliderId { get; set; }

    public List<InstaStoryVoterItem> Voters { get; set; } = new();

    public string MaxId { get; set; }

    public bool MoreAvailable { get; set; }

    public DateTime LatestSliderVoteTime { get; set; }
}