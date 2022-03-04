﻿#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaSuggestionItem
    {
        public InstaUserShort User { get; set; }

        public string Algorithm { get; set; }

        public string SocialContext { get; set; }

        public string Icon { get; set; }

        public string Caption { get; set; }

        public List<string> MediaIds { get; set; } = new();

        public List<string> ThumbnailUrls { get; set; } = new();

        public List<string> LargeUrls { get; set; } = new();

        public List<InstaMedia> MediaInfos { get; set; } = new();

        public float Value { get; set; }

        public bool IsNewSuggestion { get; set; }

        public string Uuid { get; set; }
    }

    public class InstaSuggestionItemList : List<InstaSuggestionItem>
    {
    }
}