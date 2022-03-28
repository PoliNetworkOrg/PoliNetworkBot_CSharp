﻿#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaVideoUpload
{
    public InstaVideoUpload()
    {
    }

    public InstaVideoUpload(InstaVideo video, InstaImage videoThumbnail)
    {
        Video = video;
        VideoThumbnail = videoThumbnail;
    }

    public InstaVideo Video { get; set; }
    public InstaImage VideoThumbnail { get; set; }

    /// <summary>
    ///     User tags => Optional
    /// </summary>
    public List<InstaUserTagVideoUpload> UserTags { get; set; } = new();
}