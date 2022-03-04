﻿#region

using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Media.MediaProperties;
using Windows.Storage;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Classes
{
    public class StorageUploadItem
    {
        public List<InstaUserTagUpload> UserTags = new();
        public StorageFile ImageToUpload { get; set; }
        public StorageFile VideoToUpload { get; set; }
        public bool DisableComments { get; set; } = false;
        public Rect Rect { get; set; }
        public InstaLocationShort Location { get; set; } = null;
        public bool AudioMuted { get; set; } = false;
        public string UploadId { get; set; }
        public string Caption { get; set; }

        public VideoEncodingQuality VideoEncodingQuality { get; set; } = VideoEncodingQuality.Auto;

        public TimeSpan Duration => EndTime - StartTime;

        // Current file whatever is video or image
        public uint PixelWidth { get; set; }
        public uint PixelHeight { get; set; }

        public bool IsAlbum { get; set; } = false;
        public bool IsStory { get; set; } = false;


        public Size Size => new(PixelWidth, PixelHeight);
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        internal bool IsImage => ImageToUpload != null && VideoToUpload == null;
        internal bool IsVideo { get; private set; }
        internal bool IsBoth => ImageToUpload != null && VideoToUpload != null;

        public void ThisIsVideo(bool isVideo)
        {
            IsVideo = isVideo;
        }
    }
}