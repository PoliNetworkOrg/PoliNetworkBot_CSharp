#region

using InstagramApiSharp.Enums;
using System.Collections.Generic;

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