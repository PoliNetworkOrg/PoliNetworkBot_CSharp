#region

using System.Collections.Generic;
using InstagramApiSharp.Enums;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaVoiceMedia
    {
        public InstaVoice Media { get; set; }

        public List<long> SeenUserIds { get; set; } = new();

        public InstaViewMode ViewMode { get; set; }

        public int? SeenCount { get; set; }

        public string ReplayExpiringAtUs { get; set; }
    }
}