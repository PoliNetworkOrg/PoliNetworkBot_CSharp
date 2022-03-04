#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaDiscoverSuggestedSearches
    {
        public string RankToken { get; set; }

        public List<InstaDiscoverSearches> Suggested { get; set; } = new();
    }
}