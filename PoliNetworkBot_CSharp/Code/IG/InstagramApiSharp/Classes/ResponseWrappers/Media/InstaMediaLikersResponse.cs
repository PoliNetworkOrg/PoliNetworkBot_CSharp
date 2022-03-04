#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaMediaLikersResponse : BadStatusResponse
    {
        [JsonProperty("users")] public List<InstaUserShortResponse> Users { get; set; }

        [JsonProperty("user_count")] public int UsersCount { get; set; }
    }
}