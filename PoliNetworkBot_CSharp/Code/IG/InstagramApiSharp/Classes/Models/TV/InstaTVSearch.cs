#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaTVSearch
    {
        public List<InstaTVSearchResult> Results { get; set; } = new();

        public int NumResults { get; set; }

        public string RankToken { get; set; }

        internal string Status { get; set; }
    }

    public class InstaTVSearchResult
    {
        public string Type { get; set; }

        public InstaUserShortFriendship User { get; set; }

        public InstaTVChannel Channel { get; set; }
    }
}