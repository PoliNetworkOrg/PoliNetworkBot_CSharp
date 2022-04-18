#region

using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Story;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaStoryUploadOptions
{
    public List<InstaStoryLocationUpload> Locations { get; set; } = new();

    public List<InstaStoryHashtagUpload> Hashtags { get; set; } = new();

    public List<InstaStoryPollUpload> Polls { get; set; } = new();

    public InstaStorySliderUpload Slider { get; set; }

    public InstaStoryCountdownUpload Countdown { get; set; }

    internal InstaMediaStoryUpload MediaStory { get; set; }

    public List<InstaStoryMentionUpload> Mentions { get; set; } = new();

    public List<InstaStoryQuestionUpload> Questions { get; set; } = new();
}