#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters;

internal class
    InstaStorySliderStickerItemConverter : IObjectConverter<InstaStorySliderStickerItem,
        InstaStorySliderStickerItemResponse>
{
    public InstaStorySliderStickerItemResponse SourceObject { get; set; }

    public InstaStorySliderStickerItem Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var slider = new InstaStorySliderStickerItem
        {
            Emoji = SourceObject.Emoji,
            Question = SourceObject.Question,
            SliderId = SourceObject.SliderId,
            SliderVoteAverage = SourceObject.SliderVoteAverage ?? 0,
            SliderVoteCount = SourceObject.SliderVoteCount ?? 0,
            TextColor = SourceObject.TextColor,
            ViewerCanVote = SourceObject.ViewerCanVote
        };
        return slider;
    }
}