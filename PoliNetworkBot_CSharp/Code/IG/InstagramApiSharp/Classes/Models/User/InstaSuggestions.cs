#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaSuggestions
    {
        public bool MoreAvailable { get; set; }

        public string NextMaxId { get; set; }

        public List<InstaSuggestionItem> SuggestedUsers { get; set; } = new();

        public List<InstaSuggestionItem> NewSuggestedUsers { get; set; } = new();
    }
}