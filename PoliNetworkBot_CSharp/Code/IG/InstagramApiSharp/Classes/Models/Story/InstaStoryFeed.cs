﻿#region

using InstagramApiSharp.Classes.Models.Hashtags;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaStoryFeed
    {
        public int FaceFilterNuxVersion { get; set; }

        public bool HasNewNuxStory { get; set; }

        public string StoryRankingToken { get; set; }

        public int StickerVersion { get; set; }

        public List<InstaReelFeed> Items { get; set; } = new();

        public List<InstaBroadcast> Broadcasts { get; set; } = new();

        public List<InstaBroadcastAddToPostLive> PostLives { get; set; } = new();

        public List<InstaHashtagStory> HashtagStories { get; set; } = new();
    }
}