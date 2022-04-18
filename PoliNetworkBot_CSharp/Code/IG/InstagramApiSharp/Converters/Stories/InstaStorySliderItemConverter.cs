﻿#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaStorySliderItemConverter : IObjectConverter<InstaStorySliderItem, InstaStorySliderItemResponse>
{
    public InstaStorySliderItemResponse SourceObject { get; set; }

    public InstaStorySliderItem Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var Slider = new InstaStorySliderItem
        {
            Height = SourceObject.Height,
            IsHidden = SourceObject.IsHidden,
            IsPinned = SourceObject.IsPinned,
            Rotation = SourceObject.Rotation,
            Width = SourceObject.Width,
            X = SourceObject.X,
            Y = SourceObject.Y,
            Z = SourceObject.Z
        };
        if (SourceObject.SliderSticker != null)
            Slider.SliderSticker = ConvertersFabric.GetStorySliderStickerItemConverter(SourceObject.SliderSticker)
                .Convert();

        return Slider;
    }
}