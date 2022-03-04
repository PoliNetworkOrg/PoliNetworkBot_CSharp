﻿#region

using System;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models.Hashtags
{
    public class InstaHashtagStory
    {
        public string Id { get; set; }

        public int LatestReelMedia { get; set; }

        public DateTime ExpiringAt { get; set; }

        public bool CanReply { get; set; }

        public bool CanReshare { get; set; }

        public string ReelType { get; set; }

        public InstaHashtagOwner Owner { get; set; }

        public List<InstaStoryItem> Items { get; set; } = new();

        public int PrefetchCount { get; set; }

        public long UniqueIntegerReelId { get; set; }

        public bool Muted { get; set; }
    }

    public class InstaHashtagOwner
    {
        public string Type { get; set; }

        public string Pk { get; set; }

        public string Name { get; set; }

        public string ProfilePicUrl { get; set; }

        public string ProfilePicUsername { get; set; }
    }
}