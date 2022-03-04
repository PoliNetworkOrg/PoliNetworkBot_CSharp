#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaStoryCountdownList
    {
        public List<InstaStoryCountdownStickerItem> Items { get; set; } = new();

        public bool MoreAvailable { get; set; }

        public string MaxId { get; set; }
    }
}