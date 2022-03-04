#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaContactUserListResponse
    {
        [JsonProperty("status")] public string Status { get; set; }

        [JsonProperty("items")] public List<InstaContactUserResponse> Items { get; set; }
    }

    public class InstaContactUserResponse
    {
        [JsonProperty("user")] public InstaUserContactResponse User { get; set; }
    }

    public class InstaUserContactResponse : InstaUserShortResponse
    {
        [JsonProperty("extra_display_name")] public string ExtraDisplayName { get; set; }
    }
}