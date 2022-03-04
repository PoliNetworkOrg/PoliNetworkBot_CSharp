#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaActivityFeed : IInstaBaseList
    {
        public bool IsOwnActivity { get; set; } = false;
        public List<InstaRecentActivityFeed> Items { get; set; } = new();
        public string NextMaxId { get; set; }
    }
}