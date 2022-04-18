#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.Models.Hashtags;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Story;

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