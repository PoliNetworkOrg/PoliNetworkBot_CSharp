#region

using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaBlockedCommentersResponse : InstaDefault
    {
        [JsonProperty("count")] public int Count { get; set; }

        [JsonProperty("blocked_commenters")] public List<InstaUserShortResponse> BlockedCommenters { get; set; }
    }
}