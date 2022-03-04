#region

using System;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaHighlightShortList
    {
        public List<InstaHighlightShort> Items { get; set; } = new();

        public int ResultsCount { get; set; }

        public bool MoreAvailable { get; set; }

        public string MaxId { get; set; }
    }

    public class InstaHighlightShort
    {
        public DateTime Time { get; set; }

        public int MediaCount { get; set; }

        public string Id { get; set; }

        public string ReelType { get; set; }

        public int LatestReelMedia { get; set; }
    }
}