#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaDiscoverSearchResult
    {
        public int NumResults { get; set; }

        public List<InstaUser> Users { get; set; } = new();

        public bool HasMoreAvailable { get; set; }

        public string RankToken { get; set; }
    }
}