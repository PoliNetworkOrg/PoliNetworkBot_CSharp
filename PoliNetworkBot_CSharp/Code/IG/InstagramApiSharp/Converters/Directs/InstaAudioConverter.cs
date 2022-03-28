﻿#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaAudioConverter : IObjectConverter<InstaAudio, InstaAudioResponse>
{
    public InstaAudioResponse SourceObject { get; set; }

    public InstaAudio Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");

        var audio = new InstaAudio
        {
            AudioSource = SourceObject.AudioSource,
            Duration = SourceObject.Duration,
            WaveformData = SourceObject.WaveformData,
            WaveformSamplingFrequencyHz = SourceObject.WaveformSamplingFrequencyHz
        };

        return audio;
    }
}