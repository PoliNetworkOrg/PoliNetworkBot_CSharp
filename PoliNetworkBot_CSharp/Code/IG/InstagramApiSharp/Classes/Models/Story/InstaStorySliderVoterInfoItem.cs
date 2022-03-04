﻿#region

using System;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaStorySliderVoterInfoItem
    {
        public long SliderId { get; set; }

        public List<InstaStoryVoterItem> Voters { get; set; } = new();

        public string MaxId { get; set; }

        public bool MoreAvailable { get; set; }

        public DateTime LatestSliderVoteTime { get; set; }
    }
}